using System.Net;
using System.Net.NetworkInformation;

namespace Convenient.Gooday.Net
{
    internal class InterfaceInfo
    {
        internal int Index { get; }
        internal IPAddress Ipv4Address { get; }
        internal NetworkInterface Interface { get; }

        internal InterfaceInfo(int index, IPAddress ipv4Address, NetworkInterface @interface)
        {
            Index = index;
            Ipv4Address = ipv4Address;
            Interface = @interface;
        }
    }
}