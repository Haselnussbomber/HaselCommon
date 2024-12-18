using System.Linq;
using System.Text;

namespace HaselCommon.Extensions.Strings;

public static class StringExtensions
{
    //! https://stackoverflow.com/a/4405876
    public static string FirstCharToUpper(this string input)
        => string.IsNullOrEmpty(input) ? string.Empty : string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));

    public static string FirstCharToLower(this string input)
        => string.IsNullOrEmpty(input) ? string.Empty : string.Concat(input[0].ToString().ToLower(), input.AsSpan(1));

    public static string StripSoftHypen(this string input)
        => input.Replace("\u00AD", string.Empty);

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
