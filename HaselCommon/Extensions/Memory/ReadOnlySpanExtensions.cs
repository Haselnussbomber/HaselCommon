namespace HaselCommon.Extensions.Memory;

public static class ReadOnlySpanExtensions
{
    public static ReadOnlySpan<byte> WithNullTerminator(this ReadOnlySpan<byte> input)
    {
        var len = input.Length;

        if (len > 0 && input[^1] == 0)
            return input;

        var output = new byte[len + 1];
        input.CopyTo(output);
        output[len] = 0;
        return output.AsSpan();
    }
}
