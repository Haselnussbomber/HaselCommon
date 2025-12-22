namespace HaselCommon.Extensions;

public static class ReadOnlySpanExtensions
{
    extension(ReadOnlySpan<byte> value)
    {
        public byte[] WithNullTerminator()
        {
            var len = value.Length;

            if (len > 0 && value[^1] == 0)
                return value.ToArray();

            var output = new byte[len + 1];
            value.CopyTo(output);
            output[len] = 0;
            return output;
        }
    }
}
