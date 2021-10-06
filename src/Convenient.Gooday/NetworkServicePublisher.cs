using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    /// Publishes a service with multicast DNS 
    /// </summary>
    public class NetworkServicePublisher: IDisposable
    {
        private readonly ILogger _logger;
        public bool IsRunning { get; private set; }
        
        private readonly List<MulticastClient> _clients;
        private Task _run;
        public ServiceIdentifier Id { get; }
        public ushort Port { get; }
        public string Host { get; }
        private DateTimeOffset _nextPublishTime = DateTimeOffset.UtcNow;
        private readonly Dictionary<string, string> _txtRecord;
        
        /// <summary>
        /// Creates a NetworkServicePublisher
        /// </summary>
        /// <param name="instanceName">Display name, e.g My laptop</param>
        /// <param name="serviceType">e.g _music-service._tcp</param>
        /// <param name="domain">e.g local</param>
        /// <param name="port">e.g tcp port</param>
        public NetworkServicePublisher(string instanceName, string serviceType, string domain, ushort port)
            : this(instanceName, serviceType, domain, port, new Dictionary<string, string>(), new NullLogger())
        {
        }
        
        /// <summary>
        /// Creates a NetworkServicePublisher
        /// </summary>
        /// <param name="instanceName">Display name, e.g My laptop</param>
        /// <param name="serviceType">e.g _music-service._tcp</param>
        /// <param name="domain">e.g local</param>
        /// <param name="port">e.g tcp port</param>
        /// <param name="txtRecord">Additional text info</param>
        public NetworkServicePublisher(string instanceName, string serviceType, string domain, ushort port, Dictionary<string, string> txtRecord)
            : this(instanceName, serviceType, domain, port, txtRecord, new NullLogger())
        {
        }
        
        
        /// <summary>
        /// Creates a NetworkServicePublisher
        /// </summary>
        /// <param name="instanceName">Display name, e.g My laptop</param>
        /// <param name="serviceType">e.g _music-service._tcp</param>
        /// <param name="domain">e.g local</param>
        /// <param name="port">e.g tcp port</param>
        /// <param name="txtRecord">Additional text info</param>
        /// <param name="logger">Logger</param>
        public NetworkServicePublisher(string instanceName, string serviceType, string domain, ushort port, Dictionary<string, string> txtRecord, ILogger logger)
        {
            Id = new ServiceIdentifier(instanceName.WithoutPostfix(), serviceType.UnderscorePrefix(), domain);
            Port = port;
            Host = Guid.NewGuid().ToString("N");
            _clients = Zeroconf.CreateMulticastClients().ToList();
            _logger = logger ?? new NullLogger();
            _txtRecord = txtRecord ?? new Dictionary<string, string>();
        }

        public void Start()
        {
            IsRunning = true;
            _run = RunAsync();
            _logger.Info($"Start publishing {Id} on port {Port}");
        }

        public async Task StopAsync()
        {
            try
            {
                _logger.Info($"Stop publishing {Id}. on port {Port}");
                IsRunning = false;
                await Task.WhenAll(_clients.Select(SendGoodbyeMessage));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }
            finally
            {
                foreach (var client in _clients.Select(c => c.Udp))
                {
                    client.Close();
                }
                _run = null;
            }
        }

        private Task RunAsync()
        {
            return Task.WhenAll(_clients.Select(PublishAsync).Concat(_clients.Select(ReceiveFromAsync)));
        }

        private async Task PublishAsync(MulticastClient client)
        {
            while (IsRunning)
            {
                try
                {
                    if (DateTimeOffset.UtcNow > _nextPublishTime)
                    {
                        var hello = CreateResponse(0, 120, client.Adapter.Ipv4Address);
                        var bytes = MessageParser.Encode(hello);
                        _logger.Trace($"Publishing hello\n{hello}");
                        await client.Udp.SendAsync(bytes, bytes.Length, Zeroconf.BroadcastEndpoint);
                        _nextPublishTime = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(110);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(9));
                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());
                }
            }
        }
        
        private async Task ReceiveFromAsync(MulticastClient client)
        {
            
            while (IsRunning)
            {
                try
                {
                    var result = await client.Udp.ReceiveAsync();
                    var message = MessageParser.Decode(result.Buffer);

                    if (message.Type == MessageType.Query
                        && message.Questions.Any(q => q.QType == QType.PTR
                        && q.QName == $"{Id.ServiceType}.{Id.Domain}."))
                    {
                        var response = CreateResponse(message.Id, 120, client.Adapter.Ipv4Address);
                        var packet = MessageParser.Encode(response);
                        await client.Udp.SendAsync(packet, packet.Length, result.RemoteEndPoint);
                    }
                }
                catch (ObjectDisposedException e)
                {
                    if (e.InnerException is TaskCanceledException)
                    {
                    }
                }
                catch (Exception e)
                {
                    _logger.Write(LogImportance.Important, LogMessageType.Error, e.ToString());
                }
            }
        }

        private Task SendGoodbyeMessage(MulticastClient client)
        {
            var goodbye = CreateGoodbyeMessage();
            var data = MessageParser.Encode(goodbye);
            _logger.Trace($"Publishing goodbye\n{goodbye}");
            return client.Udp.SendAsync(data, data.Length, Zeroconf.BroadcastEndpoint);
        }
        
        private DomainMessage CreateGoodbyeMessage()
        {
            return new DomainMessage
            {
                Type = MessageType.Response,
                AuthoriativeAnswer = true,
                Answers =
                {
                    new ResourceRecord
                    {
                        Name = $"{Id.ServiceType}.{Id.Domain}.",
                        Type = RRType.PTR, 
                        Class = Class.IN,
                        Ttl = 1,
                        Record = new PTRRecord
                        {
                            PTRDName = Id.Format()
                        }
                    }
                }
            };
        }

        private DomainMessage CreateResponse(ushort id, uint ttl, IPAddress ipAddress)
        {
            return new DomainMessage
            {
                Id = id,
                Type = MessageType.Response,
                AuthoriativeAnswer = true,
                Answers = new List<ResourceRecord>
                {
                    new ResourceRecord
                    {
                        Name = $"_services._dns-sd._udp.{Id.Domain}.",
                        Type = RRType.PTR, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new PTRRecord
                        {
                            PTRDName = $"{Id.ServiceType}.{Id.Domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = Id.Format(),
                        Type = RRType.TXT, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new TXTRecord
                        {
                            Text = _txtRecord.Select(p => $"{p.Key}={p.Value}").ToList()
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{Id.ServiceType}.{Id.Domain}.",
                        Type = RRType.PTR, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new PTRRecord
                        {
                            PTRDName = Id.Format()
                        }
                    },
                    new ResourceRecord
                    {
                        Name = Id.Format(),
                        Type = RRType.SRV, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new SRVRecord
                        {
                            Priority = 0,
                            Weight = 0,
                            Port = Port,
                            Target = $"{Host}.{Id.Domain}."
                        }
                    }
                },
                Additionals =
                {
                    new ResourceRecord
                    {
                        Name = $"{Host}.{Id.Domain}.",
                        Type = RRType.A,
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new ARecord
                        {
                            Address = ipAddress.MapToIPv4().ToString()
                        }
                    }
                }
            };
        }

        public void Dispose()
        {
            StopAsync().Wait(2000);
        }
    }
}