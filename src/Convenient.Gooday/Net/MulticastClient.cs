using System.Net.Sockets;

namespace Convenient.Gooday.Net
{
    internal class MulticastClient
    {
        internal UdpClient Udp { get; }
        internal InterfaceInfo Adapter { get; }
        
        internal MulticastClient(UdpClient udp, InterfaceInfo adapter)
        {
            Udp = udp;
            Adapter = adapter;
        }
    }
}