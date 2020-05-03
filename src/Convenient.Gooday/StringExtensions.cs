namespace Convenient.Gooday
{
    internal static class StringExtensions
    {
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