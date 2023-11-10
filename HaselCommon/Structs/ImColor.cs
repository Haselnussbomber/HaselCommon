using System.Numerics;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Structs;

public struct ImColor
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public ImColor()
    {
    }

    public ImColor(float r, float g, float b, float a = 1)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public ImColor(Vector4 vec) : this(vec.X, vec.Y, vec.Z, vec.W)
    {
    }

    public static ImColor FromABGR(uint abgr)
    {
        var color = (ImColor)abgr;
        (color.R, color.G, color.B, color.A) = (color.A, color.B, color.G, color.R);
        return color;
    }

    public static ImColor FromUiForeground(uint id)
        => FromABGR(GetRow<UIColor>(id)!.UIForeground);

    public static ImColor FromUiGlow(uint id)
        => FromABGR(GetRow<UIColor>(id)!.UIGlow);

    public static implicit operator ImColor(uint col)
        => new(ImGui.ColorConvertU32ToFloat4(col));

    public static implicit operator ImColor(Vector4 vec)
        => new(vec);

    public static implicit operator Vector4(ImColor col)
        => new(col.R, col.G, col.B, col.A);

    public static implicit operator uint(ImColor col)
        => ImGui.ColorConvertFloat4ToU32(col);
}
