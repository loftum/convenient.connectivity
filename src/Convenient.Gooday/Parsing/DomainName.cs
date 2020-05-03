using System;
using System.Text;

namespace Convenient.Gooday.Parsing
{
    internal static class DomainName
    {
        internal static byte[] ToBytes(string src)
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
            int ii, jj, intLen = src.Length;
            sb.Append('\0');
            for (ii = 0, jj = 0; ii < intLen; ii++, jj++)
            {
                sb.Append(src[ii]);
                if (src[ii] == '.')
                {
                    sb[ii - jj] = (char)(jj & 0xff);
                    jj = -1;
                }
            }
            sb[sb.Length -1] = '\0';
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}