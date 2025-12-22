namespace HaselCommon.Extensions;

public static class CStringExtensions
{
    extension(CStringPointer ptr)
    {
        public ReadOnlySeStringSpan AsReadOnlySeStringSpan()
        {
            return ptr.AsSpan();
        }

        public ReadOnlySeString AsReadOnlySeString()
        {
            return new ReadOnlySeString(ptr.AsSpan().ToArray());
        }

        public string ExtractText()
        {
            return ptr.AsReadOnlySeStringSpan().ToString();
        }
    }
}
