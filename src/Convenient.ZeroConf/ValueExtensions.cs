using System.Linq;

namespace Convenient.ZeroConf
{
    public static class ValueExtensions
    {
        public static bool In<T>(this T value, params T[] values)
        {
            return values.Any(v => v.Equals(value));
        }
    }
}