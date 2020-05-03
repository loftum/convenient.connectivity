using System.Collections.Generic;
using System.Text;

namespace Convenient.ZeroConf.Domain.Records
{
    public class TextRecord : IRecord
    {
        public List<string> Text { get; set; } = new List<string>();

        public byte[] ToBytes()
        {
            var bytes = new List<byte>();
            foreach (var text in Text)
            {
                bytes.Add((byte)text.Length);
                bytes.AddRange(Encoding.UTF8.GetBytes(text));
            }

            return bytes.ToArray();
        }
        
        public override string ToString()
        {
            return string.Join(" ", Text);
        }
    }
}