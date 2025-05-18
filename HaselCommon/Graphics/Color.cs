using System.Buffers.Binary;

namespace HaselCommon.Graphics;

public struct Color
{
    public float R;
    public float G;
    public float B;
    public float A;

    public Color(float r, float g, float b, float a = 1) : this()
    {
        R = r;
        G = g;
        B = b;
        A = a;
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

    public static Color FromRGBA(uint rgba)
        => FromVector4(ImGui.ColorConvertU32ToFloat4(rgba));

    public static Color FromBGRA(uint bgra)
        => FromRGBA((bgra & 0xFF00FF00) | ((bgra & 0x000000FF) << 16) | ((bgra & 0x00FF0000) >> 16)); // swap red and blue channel

    public static Color FromABGR(uint abgr)
        => FromRGBA(BinaryPrimitives.ReverseEndianness(abgr));

    public static Color FromARGB(uint argb)
        => FromRGBA(BinaryPrimitives.ReverseEndianness((argb & 0xFF00FF00) | ((argb & 0x000000FF) << 16) | ((argb & 0x00FF0000) >> 16))); // swap red and blue channel, then endianness

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
        return new(
            GetChannelColor(0, hue, lightness, chroma),   // Red channel
            GetChannelColor(8, hue, lightness, chroma),   // Green channel
            GetChannelColor(4, hue, lightness, chroma),   // Blue channel
            alpha                                         // Alpha channel
        );
    }

    private struct Matrix3x3(
        float m11, float m12, float m13,
        float m21, float m22, float m23,
        float m31, float m32, float m33)
    {
        public Vector3 Multiply(Vector3 vec)
            => new(
                m11 * vec.X + m12 * vec.Y + m13 * vec.Z,
                m21 * vec.X + m22 * vec.Y + m23 * vec.Z,
                m31 * vec.X + m32 * vec.Y + m33 * vec.Z);
    }

    //! https://github.com/color-js/color.js/blob/6332f3a/src/spaces/oklab.js
    private static readonly Matrix3x3 LABtoLMS = new(
        1f, 0.3963377773761749f, 0.2158037573099136f,
        1f, -0.1055613458156586f, -0.0638541728258133f,
        1f, -0.0894841775298119f, -1.2914855480194092f
    );

    private static readonly Matrix3x3 LMStoXYZ = new(
        1.2268798758459243f, -0.5578149944602171f, 0.2813910456659647f,
        -0.0405757452148008f, 1.1122868032803170f, -0.0717110580655164f,
        -0.0763729366746601f, -0.4214933324022432f, 1.5869240198367816f
    );

    // https://github.com/color-js/color.js/blob/6332f3a/src/spaces/srgb-linear.js
    private static readonly Matrix3x3 SRGBLinearfromXYZ = new(
        3.2409699419045226f, -1.537383177570094f, -0.4986107602930034f,
        -0.9692436362808796f, 1.8759675015077202f, 0.04155505740717559f,
        0.05563007969699366f, -0.20397695888897652f, 1.0569715142428786f
    );

    // https://gist.github.com/dkaraush/65d19d61396f5f3cd8ba7d1b4b3c9432
    /// <remarks> For sRGB the chroma value will be always below 0.37. </remarks>
    public static Color FromOKLCH(float lightness, float chroma, float hue, float alpha = 1f)
    {
        var lab = new Vector3(
            lightness,
            float.IsNaN(hue) ? 0 : chroma * MathF.Cos(hue * MathF.PI / 180),
            float.IsNaN(hue) ? 0 : chroma * MathF.Sin(hue * MathF.PI / 180));

        var lmsg = LABtoLMS.Multiply(lab);
        var lms = lmsg.Pow(3);
        var xyz = LMStoXYZ.Multiply(lms);
        var srgbLinear = SRGBLinearfromXYZ.Multiply(xyz);

        static float srgbLinear2rgb(float c)
            => MathF.Abs(c) > 0.0031308f
                ? (c < 0 ? -1 : 1) * (1.055f * MathF.Pow(MathF.Abs(c), 1 / 2.4f) - 0.055f)
                : 12.92f * c;

        return new(
            srgbLinear2rgb(srgbLinear.X),
            srgbLinear2rgb(srgbLinear.Y),
            srgbLinear2rgb(srgbLinear.Z),
            alpha
        );
    }

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

    public static Color Transparent { get; } = new(0, 0, 0, 0);
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
    public static Color Grey { get; } = new(0.73f, 0.73f, 0.73f);
    public static Color Grey2 { get; } = new(0.87f, 0.87f, 0.87f);
    public static Color Grey3 { get; } = new(0.6f, 0.6f, 0.6f);
    public static Color Grey4 { get; } = new(0.3f, 0.3f, 0.3f);
}
