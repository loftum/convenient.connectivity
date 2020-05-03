using Convenient.Gooday.Domain.Types;

namespace Convenient.Gooday.Domain.Records
{
    public class UnknownRecord : IRecord
    {
        public byte[] RData { get; set; }
        public RRType Type { get; set; }

        public override string ToString()
        {
            return $"Unknown: {Type}";
        }

        public byte[] ToBytes()
        {
            return RData;
        }
    }
}