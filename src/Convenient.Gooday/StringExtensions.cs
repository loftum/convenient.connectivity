namespace Convenient.Gooday
{
    static class StringExtensions
    {
        public static string UnderscorePrefix(this string value)
        {
            return value == null || value.StartsWith("_") 
                ? value
                : $"_{value}";
        }

        public static string WithoutPostfix(this string value)
        {
            return value == null || !value.Contains(".")
                ? value
                : value.Substring(0, value.IndexOf('.'));
        }
    }
}