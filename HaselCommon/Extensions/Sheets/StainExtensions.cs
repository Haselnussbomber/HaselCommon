using System.Buffers.Binary;

namespace HaselCommon.Extensions;

public static class StainExtensions
{
    extension(Stain row)
    {
        public Color GetColor()
        {
            return Color.FromRGBA(BinaryPrimitives.ReverseEndianness(row.Color) >> 8) with { A = 1f };
        }
    }
}
