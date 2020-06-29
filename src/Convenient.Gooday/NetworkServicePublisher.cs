using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Convenient.Gooday.Domain;
using Convenient.Gooday.Domain.Records;
using Convenient.Gooday.Domain.Types;
using Convenient.Gooday.Logging;
using Convenient.Gooday.Net;
using Convenient.Gooday.Parsing;

namespace Convenient.Gooday
{
    public class NetworkServicePublisher: IDisposable
    {
        private readonly ILogger _logger;
        public bool IsRunning { get; private set; }
        
        private readonly List<MulticastClient> _clients;
        private Task _run;

        public string InstanceName { get; }
        public string ServiceType { get; }
        public string Domain { get; }
        public ushort Port { get; }
        public string Host { get; }
        private DateTimeOffset _nextPublishTime = DateTimeOffset.UtcNow;
        
        public NetworkServicePublisher(string instanceName, string serviceType, ushort port, string domain = "local", ILogger logger = null)
        {
            InstanceName = instanceName.WithoutPostfix();
            ServiceType = serviceType.UnderscorePrefix();
            Domain = domain;
            Port = port;
            Host = Guid.NewGuid().ToString("N");
            _clients = Zeroconf.CreateMulticastClients().ToList();
            _logger = logger ?? new NullLogger();
        }

        public void Start()
        {
            IsRunning = true;
            _run = RunAsync();
            _logger.Info($"Start publishing {InstanceName}.{ServiceType}.{Host}.{Domain}. on port {Port}");
        }

        public async Task StopAsync()
        {
            try
            {
                _logger.Info($"Stop publishing {InstanceName}.{ServiceType}.{Host}.{Domain}. on port {Port}");
                IsRunning = false;
                await Task.WhenAll(_clients.Select(SendGoodbyeMessage));
            }
            catch (Exception)
            {

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
                        var hello = CreateMessage(0, 120, client.Adapter.Ipv4Address);
                        var bytes = MessageParser.Encode(hello);
                        _logger.Trace($"Publishing hello\n{hello}");
                        await client.Udp.SendAsync(bytes, bytes.Length, Zeroconf.BroadcastEndpoint);
                        _nextPublishTime = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(110);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(9));
                }
                catch (Exception)
                {
                    
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

                    if (message.Type == MessageType.Query &&
                        message.Questions.Any(q => q.QType == QType.PTR && q.QName == $"{ServiceType}.{Domain}."))
                    {
                        var response = CreateMessage(message.Id, 120, client.Adapter.Ipv4Address);
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
                catch (Exception)
                {
                    
                }
            }
        }

        private Task SendGoodbyeMessage(MulticastClient client)
        {
            var goodbye = CreateMessage(0, 0, client.Adapter.Ipv4Address);
            var data = MessageParser.Encode(goodbye);
            _logger.Trace($"Publishing goodbye\n{goodbye}");
            return client.Udp.SendAsync(data, data.Length, Zeroconf.BroadcastEndpoint);
        }

        private DomainMessage CreateMessage(ushort id, uint ttl, IPAddress ipAddress)
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
                        Name = "_services._dns-sd._udp.local",
                        Type = RRType.PTR, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new PTRRecord
                        {
                            //PTRDName = $"{Host}.{Domain}."
                            PTRDName = $"{ServiceType}.{Domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{InstanceName}.{ServiceType}.{Domain}.",
                        Type = RRType.TXT, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new TXTRecord
                        {
                            Text = new List<string>
                            {
                                $"_d={InstanceName}"
                            }
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{ServiceType}.{Domain}.",
                        Type = RRType.PTR, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new PTRRecord
                        {
                            PTRDName = $"{InstanceName}.{ServiceType}.{Domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{InstanceName}.{ServiceType}.{Domain}.",
                        Type = RRType.SRV, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new SRVRecord
                        {
                            Priority = 0,
                            Weight = 0,
                            Port = Port,
                            Target = $"{Host}.{Domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{Host}.{Domain}.",
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