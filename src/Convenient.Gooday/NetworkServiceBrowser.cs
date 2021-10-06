using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convenient.Gooday.Collections;
using Convenient.Gooday.Domain;
using Convenient.Gooday.Domain.Extensions;
using Convenient.Gooday.Domain.Records;
using Convenient.Gooday.Domain.Types;
using Convenient.Gooday.Logging;
using Convenient.Gooday.Net;
using Convenient.Gooday.Parsing;

namespace Convenient.Gooday
{
    /// <summary>
    /// Browses network for specified services
    /// </summary>
    public class NetworkServiceBrowser: IDisposable
    {
        public event EventHandler<NetworkServiceEventArgs> FoundService;
        public event EventHandler<NetworkServiceEventArgs> RemovedService;

        private readonly ILogger _logger;
        private readonly ExpiringDictionary<string, NetworkService> _services = new();
        
        
        public bool IsRunning { get; private set; }
        private readonly List<MulticastClient> _clients;
        private Task _receiveTask;
        private Task _sendTask;

        private readonly string _serviceType;
        private readonly string _domain;
        
        /// <summary>
        /// Creates NetworkServiceBrowser
        /// </summary>
        /// <param name="serviceType">e.g _music-service._tcp</param>
        /// <param name="domain">e.g local</param>
        public NetworkServiceBrowser(string serviceType, string domain)
            :this(serviceType, domain, new NullLogger())
        {
        }
        
        /// <summary>
        /// Creates NetworkServiceBrowser
        /// </summary>
        /// <param name="serviceType">e.g _music-service._tcp</param>
        /// <param name="domain">e.g local</param>
        /// <param name="logger">Logger</param>
        public NetworkServiceBrowser(string serviceType, string domain, ILogger logger)
        {
            _serviceType = serviceType.UnderscorePrefix();
            _domain = domain;
            _clients = Zeroconf.CreateMulticastClients().ToList();
            _services.ItemAdded += OnServiceAdded;
            _services.ItemRemoved += OnServiceRemoved;
            _logger = logger ?? new NullLogger();
        }

        private void OnServiceRemoved(object sender, CacheItemEventArgs<NetworkService> e)
        {
            RemovedService?.Invoke(this, new NetworkServiceEventArgs(e.Value));
        }

        private void OnServiceAdded(object sender, CacheItemEventArgs<NetworkService> e)
        {
            FoundService?.Invoke(this, new NetworkServiceEventArgs(e.Value));
        }

        public void Stop()
        {
            IsRunning = false;
            foreach (var client in _clients)
            {
                client.Udp.Close();
            }
        }

        public void Start()
        {
            IsRunning = true;
            _sendTask = SendAsync();
            _receiveTask = ReceiveAsync();
        }

        private async Task SendAsync()
        {
            ushort ii = 0;
            do
            {
                ii = ii == ushort.MaxValue ? (ushort)0 : (ushort)(ii + 1);
                var request = CreateRequest(ii);
                _logger.Trace($"Sending {request}");
                var bytes = new MessageWriter().Write(request);
                await Task.WhenAll(_clients.Select(c => c.Udp).Select(c => c.SendAsync(bytes, bytes.Length, Zeroconf.BroadcastEndpoint)));
                await Task.Delay(10000);
            } while (IsRunning);
        }

        private Task ReceiveAsync()
        {
            return Task.WhenAll(_clients.Select(c => c.Udp).Select(ReceiveFromAsync));
        }

        private async Task ReceiveFromAsync(UdpClient client)
        {
            while (IsRunning)
            {
                try
                {
                    var result = await client.ReceiveAsync();
                    var message = MessageParser.Decode(result.Buffer);
                    _logger.Trace($"Got\n{message}");
                    
                    if (message.Type == MessageType.Response &&
                        message.AuthoriativeAnswer &&
                        message.ResponseCode == ResponseCode.NoError)
                    {
                        Handle(message);    
                    }
                    
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }
        }

        private void Handle(DomainMessage message)
        {
            if (message.Answers.Any(a => a.Name.Contains(_serviceType)))
            {
                _logger.Debug($"Handle {message}");
            }
            else
            {
                _logger.Trace($"Handle {message}");    
            }
            

            // Find pointer (PTR) ...
            var ptr = message.Answers.FirstOrDefault(a => a.Type == RRType.PTR &&
                                                          a.Class == Class.IN &&
                                                          a.Name == $"{_serviceType}.{_domain}.");
            if (ptr == null)
            {
                return;
            }

            var pointerRecord = (PTRRecord) ptr.Record;
            
            // ... that points to a service (SRV)
            var srv = message.Answers.FirstOrDefault(a => a.Type == RRType.SRV &&
                                                          a.Name == pointerRecord.PTRDName);
            if (srv == null)
            {
                _services.SetTtl(pointerRecord.PTRDName, ptr.Ttl);
                return;
            }

            var serviceRecord = (SRVRecord) srv.Record;
            
            var service = new NetworkService
            {
                Name = srv.Name.Split('.')[0],
                Type = _serviceType,
                Domain = _domain,
                Port = serviceRecord.Port,
                HostName = serviceRecord.Target,
            };
            
            var txt = message.Answers.FirstOrDefault(a => a.Type == RRType.TXT);
            var txtRecord = (TXTRecord) txt?.Record;
            if (txtRecord?.Text != null)
            {
                service.TxtRecord.AddRange(txtRecord.Text);
            }
            
            var a = message.Answers.FirstOrDefault(a => a.Type == RRType.A) ?? message.Additionals.FirstOrDefault(a => a.Type == RRType.A);
            var aRecord = (ARecord) a?.Record;
            if (aRecord?.Address != null)
            {
                service.Addresses.Add(aRecord.Address);
            }
            
            _logger.Debug($"AddOrUpdate {pointerRecord.PTRDName}, ttl: {srv.Ttl}");
            _services.AddOrUpdate(pointerRecord.PTRDName, service, ptr.Ttl);
        }

        private DomainMessage CreateRequest(ushort id)
        {
            return new DomainMessage
            {
                Id = id,
                Type = MessageType.Query,
                Questions = new List<Question>
                {
                    new Question
                    {
                        QName = $"{_serviceType}.{_domain}.",
                        QClass = QClass.IN,
                        QType = QType.PTR
                    }
                }
            };
        }

        public void Dispose()
        {
            IsRunning = false;
            foreach (var client in _clients)
            {
                client.Udp.Close();
                client.Udp.Dispose();
            }
        }
    }
}