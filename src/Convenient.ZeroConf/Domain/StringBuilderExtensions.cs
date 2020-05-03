using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Convenient.ZeroConf.Domain
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendIfAny<T>(this StringBuilder builder, string name, IList<T> items)
        {
            if (items.Any())
            {
                builder.AppendLine(name);
                foreach (var item in items)
                {
                    builder.AppendLine($"{item}");
                }
            }

            return builder;
        }
    }
}