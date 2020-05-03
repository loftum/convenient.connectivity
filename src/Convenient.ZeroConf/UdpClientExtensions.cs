using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Convenient.ZeroConf
{
    static class UdpClientExtensions
    {
        public static async Task<int[]> AllSendAsync(this IList<UdpClient> clients, byte[] datagram, int length, IPEndPoint endPoint)
        {
            var results = new int[clients.Count];
            await Task.WhenAll(clients.Select( async (c, i) =>
            {
                var sent = await c.SendAsync(datagram, length, endPoint);
                results[i] = sent;
            }));
            return results;
        }

        public static void AllClose(this IList<UdpClient> clients)
        {
            foreach (var client in clients)
            {
                client.Close();
            }
        }
    }
}