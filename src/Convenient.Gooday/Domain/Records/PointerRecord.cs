namespace Convenient.Gooday.Domain.Records
{
    public class PointerRecord : IRecord
    {
        public string PTRDName { get; set; }

        public override string ToString()
        {
            return $"{PTRDName}";
        }
    }
}