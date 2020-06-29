using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convenient.Gooday.Collections;
using Convenient.Gooday.Domain;
using Convenient.Gooday.Domain.Records;
using Convenient.Gooday.Domain.Types;
using Convenient.Gooday.Logging;
using Convenient.Gooday.Net;
using Convenient.Gooday.Parsing;

namespace Convenient.Gooday
{
    public class NetworkServiceBrowser: IDisposable
    {
        public event EventHandler<NetworkServiceEventArgs> FoundService;
        public event EventHandler<NetworkServiceEventArgs> RemovedService;

        private readonly ILogger _logger;
        private readonly ExpiringDictionary<string, NetworkService> _services = new ExpiringDictionary<string, NetworkService>();
        
        
        public bool IsRunning { get; private set; }
        private readonly List<MulticastClient> _clients;
        private Task _receiveTask;
        private Task _sendTask;

        private readonly string _serviceType;
        private readonly string _domain;
        
        public NetworkServiceBrowser(string serviceType, string domain = "local", ILogger logger = null)
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
                    Console.WriteLine(e);
                }
            }
        }

        private void Handle(DomainMessage message)
        {
            var srv = message.Answers.FirstOrDefault(a => a.Type == RRType.SRV &&
                                                          a.Class == Class.IN &&
                                                          a.Name.EndsWith($"{_serviceType}.{_domain}."));
            if (srv == null)
            {
                return;
            }
            
            _logger.Debug($"Got\n{message}");

            var record = (SRVRecord) srv.Record; 
            var service = new NetworkService
            {
                Name = srv.Name.Split('.')[0],
                Type = _serviceType,
                Domain = _domain,
                Port = record.Port,
                HostName = record.Target
            };
            _logger.Info($"AddOrUpdate {service.Name}, ttl: {srv.Ttl}");
            _services.AddOrUpdate(service.Name, service, (int) srv.Ttl);
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