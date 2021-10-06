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
        
        internal static string UnderscorePrefix(this string value)
        {
            return value == null || value.StartsWith("_") 
                ? value
                : $"_{value}";
        }

        internal static string WithoutPostfix(this string value)
        {
            return value == null || !value.Contains(".")
                ? value
                : value.Substring(0, value.IndexOf('.'));
        }
    }
}