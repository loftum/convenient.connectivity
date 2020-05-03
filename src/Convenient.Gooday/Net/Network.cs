using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Convenient.Gooday.Net
{
    internal static class Network
    {
        internal static IEnumerable<InterfaceInfo> GetUsableInterfaces()
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
}