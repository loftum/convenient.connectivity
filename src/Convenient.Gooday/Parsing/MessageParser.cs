using Convenient.Gooday.Domain;

namespace Convenient.Gooday.Parsing
{
    public class MessageParser
    {
        public static DomainMessage Decode(byte[] bytes)
        {
            return new MessageReader(bytes).Read();
        }

        public static byte[] Encode(DomainMessage message)
        {
            return new MessageWriter().Write(message);
        }
    }
}