namespace Convenient.Gooday.Domain.Records
{
    public class SRVRecord : IRecord
    {
        public ushort Priority { get; set; }
        public ushort Weight { get; set; }
        public ushort Port { get; set; }
        public string Target { get; set; }
        
        public override string ToString()
        {
            return $"{Priority} {Weight} {Port} {Target}";
        }
    }
}