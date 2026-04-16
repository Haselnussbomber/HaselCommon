using System.Buffers.Binary;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Graphics;

public struct Color
{
    public float R;
    public float G;
    public float B;
    public float A;

    public Color(float r, float g, float b, float a = 1)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public readonly float Luminance => 0.299f * R + 0.587f * G + 0.114f * B;

    public readonly Color LerpTo(in Color colorB, float fraction)
        => Lerp(this, colorB, fraction);

    public static Color Lerp(in Color colorA, in Color colorB, float fraction)
    {
        return new Color(
            colorA.R.LerpTo(colorB.R, fraction),
            colorA.G.LerpTo(colorB.G, fraction),
            colorA.B.LerpTo(colorB.B, fraction),
            colorA.A.LerpTo(colorB.A, fraction));
    }

    public static Color FromVector4(Vector4 vec, ColorFormat format = ColorFormat.RGBA)
        => format switch
        {
            ColorFormat.RGBA => new() { R = vec.X, G = vec.Y, B = vec.Z, A = vec.W },
            ColorFormat.BGRA => new() { B = vec.X, G = vec.Y, R = vec.Z, A = vec.W },
            ColorFormat.ARGB => new() { A = vec.X, R = vec.Y, G = vec.Z, B = vec.W },
            ColorFormat.ABGR => new() { A = vec.X, B = vec.Y, G = vec.Z, R = vec.W },
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

    public static Color FromUInt(uint value, ColorFormat format = ColorFormat.RGBA)
        => format switch
        {
            ColorFormat.RGBA => FromRGBA(value),
            ColorFormat.BGRA => FromBGRA(value),
            ColorFormat.ARGB => FromARGB(value),
            ColorFormat.ABGR => FromABGR(value),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

    public static Color From(ImGuiCol col)
        => FromRGBA(ImGui.GetColorU32(col));

    public static unsafe Color FromUIColor(uint id, bool useThemeColor = true, Color? fallback = null)
    {
        var atkStage = AtkStage.Instance();
        if (atkStage == null || atkStage->AtkUIColorHolder == null)
            return fallback ?? Black;

        var color = atkStage->AtkUIColorHolder->GetColor(useThemeColor, id);
        if (color == 0)
            return fallback ?? Black;

        return FromRGBA(color);
    }

    public static Color FromRGBA(uint rgba)
        => FromVector4(ImGui.ColorConvertU32ToFloat4(rgba));

    public static Color FromBGRA(uint bgra)
        => FromRGBA((bgra & 0xFF00FF00) | ((bgra & 0x000000FF) << 16) | ((bgra & 0x00FF0000) >> 16)); // swap red and blue channel

    public static Color FromABGR(uint abgr)
        => FromRGBA(BinaryPrimitives.ReverseEndianness(abgr));

    public static Color FromARGB(uint argb)
        => FromRGBA(BinaryPrimitives.ReverseEndianness((argb & 0xFF00FF00) | ((argb & 0x000000FF) << 16) | ((argb & 0x00FF0000) >> 16))); // swap red and blue channel, then endianness

    public ImRaii.Color Push(ImGuiCol idx, bool condition = true)
        => ImRaii.PushColor(idx, ToUInt(), condition);

    public uint ToUInt(ColorFormat format = ColorFormat.RGBA)
        => ImGui.ColorConvertFloat4ToU32(ToVector(format));

    public Vector4 ToVector(ColorFormat format = ColorFormat.RGBA)
        => format switch
        {
            ColorFormat.RGBA => new Vector4(R, G, B, A),
            ColorFormat.BGRA => new Vector4(B, G, R, A),
            ColorFormat.ARGB => new Vector4(A, R, B, B),
            ColorFormat.ABGR => new Vector4(A, B, G, R),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

    public static implicit operator Vector4(Color col)
        => new(col.R, col.G, col.B, col.A);

    #region HSV

    public static Color FromHSV(float h, float s, float v, float a = 1.0f)
    {
        var color = new Color() { A = a };
        ImGui.ColorConvertHSVtoRGB(h, s, v, ref color.R, ref color.G, ref color.B);
        return color;
    }

    public readonly (float H, float S, float V) ToHSV()
    {
        var h = 0f; // hue
        var s = 0f; // saturation
        var v = 0f; // value

        ImGui.ColorConvertRGBtoHSV(R, G, B, ref h, ref s, ref v);

        return (h, s, v);
    }

    public float Hue
    {
        readonly get => ToHSV().H;
        set
        {
            var (_, s, v) = ToHSV();
            ImGui.ColorConvertHSVtoRGB(value, s, v, ref R, ref G, ref B);
        }
    }

    public float Saturation
    {
        readonly get => ToHSV().S;
        set
        {
            var (h, _, v) = ToHSV();
            ImGui.ColorConvertHSVtoRGB(h, value, v, ref R, ref G, ref B);
        }
    }

    public float Brightness // aka. Value
    {
        readonly get => ToHSV().V;
        set
        {
            var (h, s, _) = ToHSV();
            ImGui.ColorConvertHSVtoRGB(h, s, value, ref R, ref G, ref B);
        }
    }

    public readonly Color Shade(float factor)
    {
        var (h, s, v) = ToHSV();
        return FromHSV(h, s, Math.Clamp(v * factor, 0f, 1f));
    }

    #endregion

    #region Presets

    public static Color Transparent { get; } = new();
    public static Color White { get; } = new(1, 1, 1);
    public static Color Black { get; } = new(0, 0, 0);

    public static Color Red { get; } = new(1, 0, 0);
    public static Color Green { get; } = new(0, 1, 0);
    public static Color Blue { get; } = new(0, 0, 1);
    public static Color Cyan { get; } = new(0, 1, 1);
    public static Color Magenta { get; } = new(1, 0, 1);
    public static Color Yellow { get; } = new(1, 1, 0);

    public static Color Orange { get; } = new(1, 0.6f, 0);
    public static Color Gold { get; } = new(0.847f, 0.733f, 0.49f);
    [Obsolete("Use Text700")] public static Color Grey { get; } = new(0.73f, 0.73f, 0.73f);
    [Obsolete("Use Text100")] public static Color Grey2 { get; } = new(0.87f, 0.87f, 0.87f);
    [Obsolete("Use Text600")] public static Color Grey3 { get; } = new(0.6f, 0.6f, 0.6f);
    [Obsolete("Use Text200")] public static Color Grey4 { get; } = new(0.3f, 0.3f, 0.3f);

    public static Color Text => From(ImGuiCol.Text);
    public static Color Text900 => From(ImGuiCol.Text) with { A = 0.9f };
    public static Color Text800 => From(ImGuiCol.Text) with { A = 0.8f };
    public static Color Text700 => From(ImGuiCol.Text) with { A = 0.7f };
    public static Color Text600 => From(ImGuiCol.Text) with { A = 0.6f };
    public static Color Text500 => From(ImGuiCol.Text) with { A = 0.5f };
    public static Color Text400 => From(ImGuiCol.Text) with { A = 0.4f };
    public static Color Text300 => From(ImGuiCol.Text) with { A = 0.3f };
    public static Color Text200 => From(ImGuiCol.Text) with { A = 0.2f };
    public static Color Text100 => From(ImGuiCol.Text) with { A = 0.1f };

    #endregion

    #region Dalamud Colors

    /// <inheritdoc cref="ImGuiColors.InfoForeground"/>
    public static Color InfoForeground => FromVector4(ImGuiColors.InfoForeground);

    /// <inheritdoc cref="ImGuiColors.InfoBackground"/>
    public static Color InfoBackground => FromVector4(ImGuiColors.InfoBackground);

    /// <inheritdoc cref="ImGuiColors.SuccessForeground"/>
    public static Color SuccessForeground => FromVector4(ImGuiColors.SuccessForeground);

    /// <inheritdoc cref="ImGuiColors.SuccessBackground"/>
    public static Color SuccessBackground => FromVector4(ImGuiColors.SuccessBackground);

    /// <inheritdoc cref="ImGuiColors.WarningForeground"/>
    public static Color WarningForeground => FromVector4(ImGuiColors.WarningForeground);

    /// <inheritdoc cref="ImGuiColors.WarningBackground"/>
    public static Color WarningBackground => FromVector4(ImGuiColors.WarningBackground);

    /// <inheritdoc cref="ImGuiColors.ErrorForeground"/>
    public static Color ErrorForeground => FromVector4(ImGuiColors.ErrorForeground);

    /// <inheritdoc cref="ImGuiColors.ErrorBackground"/>
    public static Color ErrorBackground => FromVector4(ImGuiColors.ErrorBackground);

    /// <inheritdoc cref="ImGuiColors.AttentionForeground"/>
    public static Color AttentionForeground => FromVector4(ImGuiColors.AttentionForeground);

    /// <inheritdoc cref="ImGuiColors.AttentionBackground"/>
    public static Color AttentionBackground => FromVector4(ImGuiColors.AttentionBackground);

    #endregion
}
