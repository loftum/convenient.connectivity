using System.Net.Sockets;

namespace Convenient.Gooday.Net
{
    internal class MulticastClient
    {
        internal UdpClient Udp { get; }
        internal NetworkInterfaceInfo Adapter { get; }
        
        internal MulticastClient(UdpClient udp, NetworkInterfaceInfo adapter)
        {
            Udp = udp;
            Adapter = adapter;
        }
    }
}