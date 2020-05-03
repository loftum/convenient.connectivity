using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Convenient.ZeroConf
{
    public static class Network
    {
        public static IEnumerable<InterfaceInfo> GetUsableInterfaces()
        {
            return from i in NetworkInterface.GetAllNetworkInterfaces()
                let ipProperties = i.GetIPProperties()
                let ipV4Properties = ipProperties.GetIPv4Properties()
                let ipv4Address = ipProperties.UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address
                where i.SupportsMulticast &&
                      i.OperationalStatus == OperationalStatus.Up &&
                      i.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                      ipV4Properties != null &&
                      ipv4Address != null
                select new InterfaceInfo(ipV4Properties.Index, ipv4Address, i);
        }
    }

    public class InterfaceInfo
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