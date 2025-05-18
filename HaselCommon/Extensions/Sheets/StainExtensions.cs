using System.Buffers.Binary;

namespace HaselCommon.Extensions;

public static class StainExtensions
{
    public static Color GetColor(this Stain row)
        => Color.FromRGBA(BinaryPrimitives.ReverseEndianness(row.Color) >> 8) with { A = 1f };
}
