using System.Text;

namespace SlothfulCrud.Extensions
{
    public static class StringExtensions
    {
        public static string CamelToHyphen(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var result = new StringBuilder();
            result.Append(char.ToLower(input[0]));

            for (var i = 1; i < input.Length; i++)
            {
                var c = input[i];
                if (char.IsUpper(c))
                {
                    result.Append('-');
                    result.Append(char.ToLower(c));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public static string ToPlural(this string singular)
        {
            if (string.IsNullOrWhiteSpace(singular))
                return singular;

            if (EndsWithAny(singular, new[] { "ay", "ey", "iy", "oy", "uy" }))
                return singular + "s";

            if (singular.EndsWith("y", StringComparison.OrdinalIgnoreCase))
                return singular.Substring(0, singular.Length - 1) + "ies";

            if (singular.EndsWith("us", StringComparison.OrdinalIgnoreCase))
                return singular.Substring(0, singular.Length - 2) + "i";

            if (singular.EndsWith("is", StringComparison.OrdinalIgnoreCase))
                return singular.Substring(0, singular.Length - 2) + "es";

            if (EndsWithAny(singular, new[] { "ch", "sh", "x", "s", "z" }))
                return singular + "es";

            if (singular.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                return singular.Substring(0, singular.Length - 1) + "ves";

            if (singular.EndsWith("fe", StringComparison.OrdinalIgnoreCase))
                return singular.Substring(0, singular.Length - 2) + "ves";

            return singular + "s";
        }

        private static bool EndsWithAny(string input, IEnumerable<string> suffixes)
        {
            return suffixes.Any(suffix => input.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }
        
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }
        
        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }
    }
}