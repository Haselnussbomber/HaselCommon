using InteropGenerator.Runtime;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Extensions.Strings;

public static class CStringExtensions
{
    public static ReadOnlySeStringSpan AsReadOnlySeStringSpan(this CStringPointer ptr)
    {
        return ptr.AsSpan();
    }

    public static ReadOnlySeString AsReadOnlySeString(this CStringPointer ptr)
    {
        return new ReadOnlySeString(ptr.AsSpan().ToArray());
    }

    public static string ExtractText(this CStringPointer ptr)
    {
        return ptr.AsReadOnlySeStringSpan().ExtractText();
    }
}
