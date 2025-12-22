using System.Text;

namespace HaselCommon.Extensions;

public static class StringExtensions
{
    extension(string input)
    {
        public string Until(string delimiter)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(delimiter))
                return input;

            var idx = input.IndexOf(delimiter, StringComparison.Ordinal);
            return idx >= 0 ? input[..idx] : input;
        }

        public string After(string delimiter)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(delimiter))
                return input;

            var idx = input.IndexOf(delimiter, StringComparison.Ordinal);
            return idx >= 0 ? input[(idx + delimiter.Length)..] : input;
        }

        public string ToKebabCase()
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var builder = new StringBuilder();

            builder.Append(char.ToLower(input.First()));

            foreach (var c in input.Skip(1))
            {
                if (char.IsUpper(c))
                {
                    builder.Append('-');
                    builder.Append(char.ToLower(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
