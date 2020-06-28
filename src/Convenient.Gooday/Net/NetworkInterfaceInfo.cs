using System.Net;
using System.Net.NetworkInformation;

namespace Convenient.Gooday.Net
{
    public class NetworkInterfaceInfo
    {
        public int Index { get; }
        public IPAddress Ipv4Address { get; }
        public NetworkInterface Interface { get; }

        internal NetworkInterfaceInfo(int index, IPAddress ipv4Address, NetworkInterface @interface)
        {
            Index = index;
            Ipv4Address = ipv4Address;
            Interface = @interface;
        }
    }
}