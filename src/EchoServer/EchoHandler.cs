using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public class EchoHandler : IDisposable
    {
        private readonly Socket _socket;
        private readonly Stream _stream;
        private Task _run;
        public bool IsRunning { get; private set; }

        public EchoHandler(Socket socket)
        {
            _socket = socket;
            _stream = new NetworkStream(_socket);
        }

        public void Start(CancellationToken cancellationToken)
        {
            IsRunning = true;
            _run = RunAsync();
        }
        
        private async Task RunAsync()
        {
            try
            {
                var buffer = new byte[4096];
                var bytes = new List<byte>();
                while (IsRunning)
                {
                    bytes.Clear();
                    var read = 0;
                    do
                    {
                        read = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    } while (read > 0 && read >= 4096);

                    await _stream.WriteAsync(bytes.ToArray());
                }
            }
            catch
            {
                return;
            }
        }

        public async Task StopAsync()
        {
            IsRunning = false;
            _stream.Close();
            _socket.Disconnect(false);
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _stream?.Dispose();
            _run?.Dispose();
        }
    }
}