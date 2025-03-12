using Lumina.Text.ReadOnly;

namespace HaselCommon.Extensions.Strings;

public static class StringPointerExtensions
{
    public static ReadOnlySeString ToReadOnlySeString(this StringPointer stringPointer)
        => new(stringPointer.AsSpan());

    public static ReadOnlySeStringSpan ToReadOnlySeStringSpan(this StringPointer stringPointer)
        => new(stringPointer.AsSpan());
}
