using System;
using System.Threading;
using System.Threading.Tasks;
using Convenient.Gooday;

namespace EchoServer
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("EchoServer");
            try
            {
                using (var source = new CancellationTokenSource())
                {
                    using (var echoServer = new EchoServer())
                    {
                        echoServer.Start(source.Token);
                        using (var publisher = new NetworkServicePublisher("EchoServer", "_echo._tcp", (ushort) echoServer.Port))
                        {
                            publisher.Start();
                            var running = true;
                            Console.CancelKeyPress += async (s, e) =>
                            {
                                await echoServer.StopAsync();
                                await publisher.StopAsync();
                                running = false;
                                Console.WriteLine("Goodbye!");
                            };
                            while (running)
                            {
                                await Task.Delay(1000, source.Token);
                            }
                        }
                    }    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
            

            return 0;
        }
    }
}