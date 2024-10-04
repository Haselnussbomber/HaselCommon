using System.Buffers.Binary;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace HaselCommon.Graphics;

public struct Color
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public Color()
    {
    }

    public Color(float r, float g, float b, float a = 1)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(Vector4 vec) : this(vec.X, vec.Y, vec.Z, vec.W)
    {
    }

    public Color(uint col) : this(ImGui.ColorConvertU32ToFloat4(col))
    {
    }

    public ImRaii.Color Push(ImGuiCol idx, bool condition = true)
        => ImRaii.PushColor(idx, (uint)this, condition);

    public static Color From(float r, float g, float b, float a = 1)
        => new() { R = r, G = g, B = b, A = a };

    public static Color From(Vector4 vec)
        => From(vec.X, vec.Y, vec.Z, vec.W);

    public static Color From(uint col)
        => From(ImGui.ColorConvertU32ToFloat4(col));

    public static Color From(ImGuiCol col)
        => From(ImGui.GetColorU32(col));

    public static Color FromABGR(uint abgr)
        => From(BinaryPrimitives.ReverseEndianness(abgr));

    //! https://stackoverflow.com/a/64090995
    public static Color FromHSL(float hue, float saturation, float lightness, float alpha = 1f)
    {
        // Calculate the chroma component
        var chroma = saturation * MathF.Min(lightness, 1 - lightness);

        // Local function to compute color channels
        static float GetChannelColor(float channelOffset, float hue, float lightness, float chroma)
        {
            var hueSegment = (channelOffset + hue / 30) % 12;
            return lightness - chroma * MathF.Max(MathF.Min(MathF.Min(hueSegment - 3, 9 - hueSegment), 1), -1);
        }

        // Calculate RGB channels using the helper function
        return From(
            GetChannelColor(0, hue, lightness, chroma),   // Red channel
            GetChannelColor(8, hue, lightness, chroma),   // Green channel
            GetChannelColor(4, hue, lightness, chroma),   // Blue channel
            alpha                                         // Alpha channel
        );
    }

    public static implicit operator Vector4(Color col)
        => new(col.R, col.G, col.B, col.A);

    public static implicit operator uint(Color col)
        => ImGui.ColorConvertFloat4ToU32(col);

    public static Color Transparent { get; } = new(Vector4.Zero);
    public static Color White { get; } = new(Vector4.One);
    public static Color Black { get; } = new(0, 0, 0);
    public static Color Red { get; } = new(1, 0, 0);
    public static Color Green { get; } = new(0, 1, 0);
    public static Color Blue { get; } = new(0, 0, 1);
    public static Color Cyan { get; } = new(0, 1, 1);
    public static Color Magenta { get; } = new(1, 0, 1);
    public static Color Yellow { get; } = new(1, 1, 0);

    public static Color Orange { get; } = new(1, 0.6f, 0);
    public static Color Gold { get; } = new(0.847f, 0.733f, 0.49f);
    public static Color Grey { get; } = new(0.73f, 0.73f, 0.73f);
    public static Color Grey2 { get; } = new(0.87f, 0.87f, 0.87f);
    public static Color Grey3 { get; } = new(0.6f, 0.6f, 0.6f);
    public static Color Grey4 { get; } = new(0.3f, 0.3f, 0.3f);
}
