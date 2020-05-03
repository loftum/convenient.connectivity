using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Convenient.Gooday.Domain;
using Convenient.Gooday.Domain.Records;
using Convenient.Gooday.Domain.Types;

namespace Convenient.Gooday
{
    public class ZeroconfHost: IDisposable
    {
        public bool IsRunning { get; private set; }
        private readonly List<MulticastClient> _clients;
        private Task _run;

        private readonly string _instanceName;
        private readonly string _serviceType;
        private readonly string _domain;
        private readonly ushort _port;
        private readonly string _host;
        
        public ZeroconfHost(string instanceName, string serviceType, ushort port, string host, string domain = "local")
        {
            _instanceName = instanceName.WithoutPostfix();
            _serviceType = serviceType.UnderscorePrefix();
            _domain = domain;
            _port = port;
            _clients = Zeroconf.CreateMulticastClients().ToList();
            _host = host;
        }

        public void Start()
        {
            IsRunning = true;
            _run = RunAsync();
        }

        public async Task StopAsync()
        {
            try
            {
                IsRunning = false;
                await Task.WhenAll(_clients.Select(SendGoodbyeMessage));
            }
            catch (Exception e)
            {

            }
            finally
            {
                foreach (var client in _clients.Select(c => c.UdpClient))
                {
                    client.Close();
                }
                _run = null;
            }
        }

        private Task RunAsync()
        {
            return Task.WhenAll(_clients.Select(ReceiveFromAsync));
        }
        
        private async Task ReceiveFromAsync(MulticastClient client)
        {
            var hello = CreateResponse(0, 120, client.Adapter.Ipv4Address);
            var bytes = MessageParser.Encode(hello);
            await client.UdpClient.SendAsync(bytes, bytes.Length, Zeroconf.BroadcastEndpoint);
            while (IsRunning)
            {
                try
                {
                    var result = await client.UdpClient.ReceiveAsync();
                    var message = MessageParser.Decode(result.Buffer);

                    if (message.Type == MessageType.Query)
                    {
                        if (message.Questions.Any(q => q.QType == QType.PTR && q.QName == $"{_serviceType}.{_domain}."))
                        {
                            var response = CreateResponse(message.Id, 120, client.Adapter.Ipv4Address);
                            var packet = MessageParser.Encode(response);
                            await client.UdpClient.SendAsync(packet, packet.Length, result.RemoteEndPoint);
                        }
                    }
                }
                catch (ObjectDisposedException e)
                {
                    if (e.InnerException is TaskCanceledException a)
                    {
                            
                    }
                }
                catch (Exception e)
                {
                    
                }
            }
        }

        private Task SendGoodbyeMessage(MulticastClient client)
        {
            var goodbye = CreateResponse(0, 0, client.Adapter.Ipv4Address);
            var data = MessageParser.Encode(goodbye);
            return client.UdpClient.SendAsync(data, data.Length, Zeroconf.BroadcastEndpoint);
        }

        private ZeroconfMessage CreateResponse(ushort id, uint ttl, IPAddress ipAddress)
        {
            return new ZeroconfMessage
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
                        Record = new PointerRecord
                        {
                            PTRDName = $"{_host}.{_domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{_instanceName}.{_serviceType}.{_domain}.",
                        Type = RRType.TXT, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new TextRecord
                        {
                            Text = new List<string>
                            {
                                $"_d={_instanceName}"
                            }
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{_serviceType}.{_domain}.",
                        Type = RRType.PTR, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new PointerRecord
                        {
                            PTRDName = $"{_instanceName}.{_serviceType}.{_domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{_instanceName}.{_serviceType}.{_domain}.",
                        Type = RRType.SRV, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new ServiceRecord
                        {
                            Priority = 0,
                            Weight = 0,
                            Port = _port,
                            Target = $"{_host}.{_domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{_host}.{_domain}.",
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