using System;
using System.Collections.Generic;
using System.Text;
using Convenient.Gooday.Domain.Extensions;
using Convenient.Gooday.Domain.Types;

namespace Convenient.Gooday.Domain
{
    public class Question
    {
        public string QName { get; set; }
        public QType QType { get; set; }
        public QClass QClass { get; set; }

        static byte[] WriteName(string src)
        {
            if (!src.EndsWith(".", StringComparison.Ordinal))
            {
                src = $"{src}.";
            }

            if (src == ".")
            {
                return new byte[1];
            }
                

            var sb = new StringBuilder();
            int intI, intJ, intLen = src.Length;
            sb.Append('\0');
            for (intI = 0, intJ = 0; intI < intLen; intI++, intJ++)
            {
                sb.Append(src[intI]);
                if (src[intI] == '.')
                {
                    sb[intI - intJ] = (char)(intJ & 0xff);
                    intJ = -1;
                }
            }
            sb[sb.Length -1] = '\0';
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public byte[] ToBytes()
        {
            var data = new List<byte>();
            data.AddRange(WriteName(QName));
            data.AddRange(((ushort) QType).ToBytes());
            data.AddRange(((ushort) QClass).ToBytes());
            return data.ToArray();
        }

        public override string ToString()
        {
            return $"{QName,-32} {QClass} {QType}";
        }
    }
}