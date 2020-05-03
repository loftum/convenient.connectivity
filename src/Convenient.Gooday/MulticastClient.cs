using System.Net.Sockets;

namespace Convenient.Gooday
{
    internal class MulticastClient
    {
        internal UdpClient UdpClient { get; }
        internal InterfaceInfo Adapter { get; }
        
        internal MulticastClient(UdpClient udpClient, InterfaceInfo adapter)
        {
            UdpClient = udpClient;
            Adapter = adapter;
        }
    }
}