using System.Buffers.Binary;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace HaselCommon.Utils;

public struct HaselColor
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public HaselColor()
    {
    }

    public HaselColor(float r, float g, float b, float a = 1)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public HaselColor(Vector4 vec) : this(vec.X, vec.Y, vec.Z, vec.W)
    {
    }

    public HaselColor(uint col) : this(ImGui.ColorConvertU32ToFloat4(col))
    {
    }

    public ImRaii.Color Push(ImGuiCol idx, bool condition = true)
        => ImRaii.PushColor(idx, (uint)this, condition);

    public readonly HaselColor WithRed(float r)
        => new(r, G, B, A);

    public readonly HaselColor WithGreen(float g)
        => new(R, g, B, A);

    public readonly HaselColor WithBlue(float b)
        => new(R, G, b, A);

    public readonly HaselColor WithAlpha(float a)
        => new(R, G, B, a);

    public static HaselColor From(float r, float g, float b, float a = 1)
        => new() { R = r, G = g, B = b, A = a };

    public static HaselColor From(Vector4 vec)
        => From(vec.X, vec.Y, vec.Z, vec.W);

    public static HaselColor From(uint col)
        => From(ImGui.ColorConvertU32ToFloat4(col));

    public static HaselColor From(ImGuiCol col)
        => From(ImGui.GetColorU32(col));

    public static HaselColor FromABGR(uint abgr)
        => From(BinaryPrimitives.ReverseEndianness(abgr));

    //! https://stackoverflow.com/a/64090995
    public static HaselColor FromHSL(float hue, float saturation, float lightness, float alpha = 1f)
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

    public static implicit operator Vector4(HaselColor col)
        => new(col.R, col.G, col.B, col.A);

    public static implicit operator uint(HaselColor col)
        => ImGui.ColorConvertFloat4ToU32(col);
}
