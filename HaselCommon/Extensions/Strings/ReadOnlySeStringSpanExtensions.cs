using System.Linq;
using FFXIVClientStructs.FFXIV.Client.System.String;
using HaselCommon.Extensions.Memory;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Extensions.Strings;

public static class ReadOnlySeStringSpanExtensions
{
    public static ReadOnlySpan<byte> GetViewAsSpan(this ReadOnlySeStringSpan rosss)
    {
        return ((ReadOnlySpan<byte>)rosss).WithNullTerminator();
    }

    public static Utf8String ToUtf8String(this ReadOnlySeStringSpan rosss)
    {
        return new(rosss.GetViewAsSpan());
    }

    public static bool IsTextOnly(this ReadOnlySeStringSpan rosss)
    {
        foreach (var payload in rosss)
        {
            if (payload.Type != ReadOnlySePayloadType.Text)
                return false;
        }

        return true;
    }
}
