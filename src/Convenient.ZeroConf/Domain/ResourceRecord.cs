using Convenient.ZeroConf.Domain.Types;

namespace Convenient.ZeroConf.Domain
{
    public class ResourceRecord
    {
        public string Name { get; set; }
        public RRType Type { get; set; }
        public Class Class { get; set; }
        public uint Ttl { get; set; }
        public IRecord Record { get; set; }
        
        public override string ToString()
        {
            return $"{Name.LimitTo(50)} {Ttl} {Class} {Type} {Record}";
        }
    }
}