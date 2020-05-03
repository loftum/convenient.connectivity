namespace Convenient.Gooday.Domain
{
    public class MessageParser
    {
        public static ZeroconfMessage Decode(byte[] bytes)
        {
            return new MessageReader(bytes).Read();
        }

        public static byte[] Encode(ZeroconfMessage message)
        {
            return new MessageWriter().Write(message);
        }
    }
}