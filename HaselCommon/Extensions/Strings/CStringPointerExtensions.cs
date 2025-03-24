using Lumina.Text.ReadOnly;
using InteropGenerator.Runtime;

namespace HaselCommon.Extensions.Strings;

public static class CStringPointerExtensions
{
    public static ReadOnlySeString ToReadOnlySeString(this CStringPointer stringPointer)
        => new(stringPointer.AsSpan());

    public static ReadOnlySeStringSpan ToReadOnlySeStringSpan(this CStringPointer stringPointer)
        => new(stringPointer.AsSpan());
}
