using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Convenient.ZeroConf
{
    public class MulticastClient
    {
        public UdpClient Client { get; }
        public InterfaceInfo Adapter { get; }
        
        public MulticastClient(UdpClient client, InterfaceInfo adapter)
        {
            Client = client;
            Adapter = adapter;
        }
    }
    
    public static class Zeroconf
    {
        public const int BroadcastPort = 5353;
        public static IPAddress BroadcastAddress { get; } = IPAddress.Parse("224.0.0.251");
        public static IPEndPoint BroadcastEndpoint { get; } = new IPEndPoint(BroadcastAddress, BroadcastPort);

        public static UdpClient CreateUdpClient()
        {
            var client = new UdpClient
            {
                EnableBroadcast = true,
                ExclusiveAddressUse = false,
                MulticastLoopback = false
            };

            var socket = client.Client;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            socket.Bind(new IPEndPoint(IPAddress.Any, BroadcastPort));
            
            client.JoinMulticastGroup(Zeroconf.BroadcastAddress);
            return client;
        }

        public static IEnumerable<MulticastClient> CreateUdpClients()
        {
            var adapters = Network.GetUsableInterfaces();
            return adapters.Select(CreateUdpClient);
        }

        public static MulticastClient CreateUdpClient(InterfaceInfo adapter)
        {
            var client = new UdpClient
            {
                ExclusiveAddressUse = false,
                MulticastLoopback = false
            };
            var socket = client.Client;
                
            socket.SetSocketOption(SocketOptionLevel.IP,
                SocketOptionName.MulticastInterface,
                IPAddress.HostToNetworkOrder(adapter.Index));

            socket.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true);
            socket.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReceiveTimeout,
                5000);
            socket.MulticastLoopback = false;

            socket.Bind(new IPEndPoint(IPAddress.Any, BroadcastPort));
                
            var multiCastOption = new MulticastOption(BroadcastAddress, adapter.Index);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multiCastOption);
            return new MulticastClient(client, adapter);
        }
    }
}