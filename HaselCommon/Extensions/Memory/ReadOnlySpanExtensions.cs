namespace HaselCommon.Extensions;

public static class ReadOnlySpanExtensions
{
    extension(ReadOnlySpan<byte> input)
    {
        public byte[] WithNullTerminator()
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
}
