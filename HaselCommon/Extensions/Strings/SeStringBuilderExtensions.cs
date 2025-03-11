using Lumina.Text;

namespace HaselCommon.Extensions.Strings;

public static class SeStringBuilderExtensions
{
    public static bool ContainsText(this SeStringBuilder builder, ReadOnlySpan<byte> needle)
    {
        return builder.ToArray().AsSpan().IndexOf(needle) != -1;
    }

    public static SeStringBuilder ReplaceText(this SeStringBuilder builder, ReadOnlySpan<byte> toFind, ReadOnlySpan<byte> replacement)
    {
        if (toFind.IsEmpty)
            return builder;

        var str = builder.ToReadOnlySeString();
        if (str.IsEmpty)
            return builder;

        builder.Clear();
        builder.Append(str.ReplaceText(toFind, replacement));

        return builder;
    }
}
