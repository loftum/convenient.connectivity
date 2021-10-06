using System.Collections.Generic;

namespace Convenient.Gooday
{
    public class NetworkService
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Domain { get; set; }
        public string HostName { get; set; }
        public List<string> Addresses { get; } = new();

        public int Port { get; set; }
        
        public List<string> TxtRecord { get; } = new();
        
        public override string ToString()
        {
            return $"{Name} {Type} {Domain} {HostName}:{Port}";
        }
    }
}