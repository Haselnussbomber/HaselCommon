using HaselCommon.Graphics;
using Lumina.Excel.Sheets;

namespace HaselCommon.Extensions.Sheets;

public static class UIColorExtensions
{
    public static Color GetForegroundColor(this UIColor row)
        => Color.FromABGR(row.Dark);

    public static Color GetEdgeColor(this UIColor row)
        => Color.FromABGR(row.Light);
}
