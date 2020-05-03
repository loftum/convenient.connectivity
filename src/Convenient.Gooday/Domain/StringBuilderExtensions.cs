using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Convenient.Gooday.Domain
{
    internal static class StringBuilderExtensions
    {
        internal static StringBuilder AppendIfAny<T>(this StringBuilder builder, string name, IList<T> items)
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