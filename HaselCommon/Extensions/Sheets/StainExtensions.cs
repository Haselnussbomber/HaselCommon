using System.Buffers.Binary;
using HaselCommon.Graphics;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Extensions.Sheets;

public static class StainExtensions
{
    public static Color GetColor(this Stain row)
        => Color.From(BinaryPrimitives.ReverseEndianness(row.Color) >> 8) with { A = 1f };
}
