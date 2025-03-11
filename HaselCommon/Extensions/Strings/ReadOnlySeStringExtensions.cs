using System.Linq;
using FFXIVClientStructs.FFXIV.Client.System.String;
using HaselCommon.Extensions.Collections;
using HaselCommon.Extensions.Memory;
using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Extensions.Strings;

public static class ReadOnlySeStringExtensions
{
    public static Utf8String ToUtf8String(this ReadOnlySeString ross)
    {
        return new(((ReadOnlySpan<byte>)ross).WithNullTerminator());
    }

    public static bool ContainsText(this ReadOnlySeString ross, ReadOnlySpan<byte> needle)
    {
        return ross.Data.Span.IndexOf(needle) != -1;
    }

    public static ReadOnlySeString ReplaceText(this ReadOnlySeString ross, ReadOnlySpan<byte> toFind, ReadOnlySpan<byte> replacement)
    {
        if (ross.IsEmpty)
            return ross;

        var sb = SeStringBuilder.SharedPool.Get();

        foreach (var payload in ross)
        {
            if (payload.Type == ReadOnlySePayloadType.Invalid)
                continue;

            if (payload.Type != ReadOnlySePayloadType.Text)
            {
                sb.Append(payload);
                continue;
            }

            var index = payload.Body.Span.IndexOf(toFind);
            if (index == -1)
            {
                sb.Append(payload);
                continue;
            }

            var lastIndex = 0;
            while (index != -1)
            {
                sb.Append(payload.Body.Span[lastIndex..index]);

                if (!replacement.IsEmpty)
                {
                    sb.Append(replacement);
                }

                lastIndex = index + toFind.Length;
                index = payload.Body.Span[lastIndex..].IndexOf(toFind);

                if (index != -1)
                    index += lastIndex;
            }

            sb.Append(payload.Body.Span[lastIndex..]);
        }

        var output = sb.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(sb);
        return output;
    }

    public static bool IsTextOnly(this ReadOnlySeString ross)
    {
        return ross.All(payload => payload.Type == ReadOnlySePayloadType.Text);
    }
}
