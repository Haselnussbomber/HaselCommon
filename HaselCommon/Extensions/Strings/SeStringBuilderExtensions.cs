using HaselCommon.Extensions;
using Lumina.Text;
using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;

namespace HaselCommon.Extensions.Strings;

// TODO: use Lumina.Text.SeStringBuilder.SharedPool.Get()/Return() everywhere

public static class SeStringBuilderExtensions
{
    public static DSeString ToDalamudString(this SeStringBuilder sb)
    {
        return DSeString.Parse(sb.ToArray());
    }

    public static bool Contains(this SeStringBuilder builder, ReadOnlySpan<byte> needle)
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
