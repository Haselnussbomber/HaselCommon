using FFXIVClientStructs.FFXIV.Client.System.String;
using HaselCommon.Extensions.Memory;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Extensions.Strings;

public static class ReadOnlySeStringExtensions
{
    public static ReadOnlySpan<byte> GetViewAsSpan(this ReadOnlySeString ross)
    {
        return ((ReadOnlySpan<byte>)ross).WithNullTerminator();
    }

    public static Utf8String ToUtf8String(this ReadOnlySeString ross)
    {
        return new(ross.GetViewAsSpan());
    }
}
