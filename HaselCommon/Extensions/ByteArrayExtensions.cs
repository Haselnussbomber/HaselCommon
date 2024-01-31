using System.IO.Hashing;

namespace HaselCommon.Extensions;

public static class ByteArrayExtensions
{
    public static string GetHash(this byte[] input)
        => BitConverter.ToInt64(XxHash3.Hash(input)).ToString("x");
}
