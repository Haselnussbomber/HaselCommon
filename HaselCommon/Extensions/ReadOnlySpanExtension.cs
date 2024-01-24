namespace HaselCommon.Extensions;

public static class ReadOnlySpanExtension
{
    public static ReadOnlySpan<byte> WithNullTerminator(this ReadOnlySpan<byte> originalSpan)
    {
        if (originalSpan.Length > 0 && originalSpan[^1] == '\0')
            return originalSpan;

        var newArray = new byte[originalSpan.Length + 1];
        originalSpan.CopyTo(newArray);
        newArray[originalSpan.Length] = 0;
        return newArray.AsSpan();
    }
}
