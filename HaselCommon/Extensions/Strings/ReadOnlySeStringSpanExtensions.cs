using Lumina.Text.ReadOnly;

namespace HaselCommon.Extensions.Strings;

public static class ReadOnlySeStringSpanExtensions
{
    public static ReadOnlySeString ToReadOnlySeString(this ReadOnlySeStringSpan rosss)
        => new(rosss.Data.ToArray());
}
