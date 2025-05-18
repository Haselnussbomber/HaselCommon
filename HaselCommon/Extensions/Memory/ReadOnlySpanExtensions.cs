namespace HaselCommon.Extensions;

public static class ReadOnlySpanExtensions
{
    public static byte[] WithNullTerminator(this ReadOnlySpan<byte> input)
    {
        var len = input.Length;

        if (len > 0 && input[^1] == 0)
            return input.ToArray();

        var output = new byte[len + 1];
        input.CopyTo(output);
        output[len] = 0;
        return output;
    }
}
