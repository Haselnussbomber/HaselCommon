using System.Buffers.Binary;
using HaselCommon.Utils;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Extensions;

public static class StainExtensions
{
    public static HaselColor GetColor(this Stain row)
        => HaselColor.From(BinaryPrimitives.ReverseEndianness(row.Color) >> 8).WithAlpha(1);
}
