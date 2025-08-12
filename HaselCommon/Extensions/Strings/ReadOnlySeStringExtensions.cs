using FFXIVClientStructs.FFXIV.Client.System.String;

namespace HaselCommon.Extensions;

public static class ReadOnlySeStringExtensions
{
    public static Utf8String ToUtf8String(this ReadOnlySeString input)
    {
        using var rssb = new RentedSeStringBuilder();
        rssb.Builder.Append(input);
        return new Utf8String(rssb.Builder.GetViewAsSpan());
    }
}
