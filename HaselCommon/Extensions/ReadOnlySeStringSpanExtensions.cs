using Lumina.Text.ReadOnly;

namespace HaselCommon.Extensions;

public static class ReadOnlySeStringSpanExtensions
{
    public static ReadOnlySeString ToReadOnlySeString(this ReadOnlySeStringSpan rosss)
        => new(rosss.Data.ToArray());
}
