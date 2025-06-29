using System.Text;

namespace HaselCommon.Extensions;

public static class StringExtensions
{
    extension(string input)
    {
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
