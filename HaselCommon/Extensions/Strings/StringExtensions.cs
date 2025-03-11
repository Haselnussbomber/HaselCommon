using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HaselCommon.Extensions.Strings;

public static class StringExtensions
{
    /// <summary>
    /// Converts the first character of the string to uppercase while leaving the rest of the string unchanged.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="culture"><inheritdoc cref="string.ToLower(CultureInfo)" path="/param[@name='cultureInfo']"/></param>
    /// <returns>A new string with the first character converted to uppercase.</returns>
    [return: NotNullIfNotNull("input")]
    public static string? FirstCharToUpper(this string? input, CultureInfo? culture = null) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : $"{char.ToUpper(input[0], culture ?? CultureInfo.CurrentCulture)}{input.AsSpan(1)}";

    /// <summary>
    /// Converts the first character of the string to lowercase while leaving the rest of the string unchanged.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="culture"><inheritdoc cref="string.ToLower(CultureInfo)" path="/param[@name='cultureInfo']"/></param>
    /// <returns>A new string with the first character converted to lowercase.</returns>
    [return: NotNullIfNotNull("input")]
    public static string? FirstCharToLower(this string? input, CultureInfo? culture = null) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : $"{char.ToLower(input[0], culture ?? CultureInfo.CurrentCulture)}{input.AsSpan(1)}";

    /// <summary>
    /// Removes soft hyphen characters (U+00AD) from the input string.
    /// </summary>
    /// <param name="input">The input string to remove soft hyphen characters from.</param>
    /// <returns>A string with all soft hyphens removed.</returns>
    public static string StripSoftHyphen(this string input) => input.Replace("\u00AD", string.Empty);

    /// <summary>
    /// Truncates the given string to the specified maximum number of characters,  
    /// appending an ellipsis if truncation occurs.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxChars">The maximum allowed length of the string.</param>
    /// <param name="ellipses">The string to append if truncation occurs (defaults to "...").</param>
    /// <returns>The truncated string, or the original string if no truncation is needed.</returns>
    public static string? Truncate(this string input, int maxChars, string ellipses = "...")
    {
        return string.IsNullOrEmpty(input) || input.Length <= maxChars ? input : input[..maxChars] + ellipses;
    }

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
