using System.Linq;
using System.Text;

namespace HaselCommon.Extensions.Strings;

public static class StringExtensions
{
    public static string ToKebabCase(this string input)
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
