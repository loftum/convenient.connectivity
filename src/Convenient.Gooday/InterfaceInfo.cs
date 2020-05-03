using System.Net;
using System.Net.NetworkInformation;

namespace Convenient.Gooday
{
    internal class InterfaceInfo
    {
        public int Index { get; }
        public IPAddress Ipv4Address { get; }
        public NetworkInterface Interface { get; }

        public InterfaceInfo(int index, IPAddress ipv4Address, NetworkInterface @interface)
        {
            Index = index;
            Ipv4Address = ipv4Address;
            Interface = @interface;
        }
    }
}