using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public class EchoServer: IDisposable
    {
        private readonly List<EchoHandler> _handlers = new List<EchoHandler>();
        private readonly TcpListener _listener;
        public int Port => ((IPEndPoint) _listener.LocalEndpoint).Port;
        private Task _run;
        
        public EchoServer()
        {
            _listener = new TcpListener(IPAddress.Any, 0); // auto-assigned port
        }

        public void Start(CancellationToken cancellationToken)
        {
            _listener.Start();
            Console.WriteLine($"EchoServer listening for connections on port {Port}");
            _run = RunAsync(cancellationToken);
        }

        public async Task StopAsync()
        {
            _listener.Stop();
            await Task.WhenAll(_handlers.Select(h => h.StopAsync()));
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _listener.AcceptSocketAsync();
                var handler = new EchoHandler(socket);
                _handlers.Add(handler);
                handler.Start(cancellationToken);
            }
        }

        public void Dispose()
        {
            StopAsync().Wait(1000);
        }
    }
}