namespace Convenient.Gooday.Domain.Extensions
{
    internal static class StringExtensions
    {
        internal static string LimitTo(this string value, int limit)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= limit)
            {
                return value;
            }

            return $"{value.Substring(0, limit - 5)}(...)";
        }
    }
}