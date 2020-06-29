using System;

namespace Convenient.Gooday
{
    public class NetworkServiceEventArgs : EventArgs
    {
        public NetworkService Service { get; }
        
        public NetworkServiceEventArgs(NetworkService service)
        {
            Service = service;
        }
    }
}