using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Convenient.Gooday
{
    internal static class Zeroconf
    {
        internal const int BroadcastPort = 5353;
        internal static IPAddress BroadcastAddress { get; } = IPAddress.Parse("224.0.0.251");
        internal static IPEndPoint BroadcastEndpoint { get; } = new IPEndPoint(BroadcastAddress, BroadcastPort);

        internal static IEnumerable<MulticastClient> CreateMulticastClients()
        {
            var adapters = Network.GetUsableInterfaces();
            return adapters.Select(CreateMulticastClient);
        }

        internal static MulticastClient CreateMulticastClient(InterfaceInfo adapter)
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