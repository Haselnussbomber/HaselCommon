using System.Numerics;

namespace HaselCommon.Math;

public struct Matrix3x3
{
    public static readonly Matrix3x3 Zero = new();
    public static readonly Matrix3x3 Identity = new() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f };

    public float M11;
    public float M12;
    public float M13;

    public float M21;
    public float M22;
    public float M23;

    public float M31;
    public float M32;
    public float M33;

    public Matrix3x3(
        float m11, float m12, float m13,
        float m21, float m22, float m23,
        float m31, float m32, float m33)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M31 = m31;
        M32 = m32;
        M33 = m33;
    }

    public static Vector3 operator *(Matrix3x3 mat, Vector3 vec)
        => new(
            mat.M11 * vec.X + mat.M12 * vec.Y + mat.M13 * vec.Z,
            mat.M21 * vec.X + mat.M22 * vec.Y + mat.M23 * vec.Z,
            mat.M31 * vec.X + mat.M32 * vec.Y + mat.M33 * vec.Z
        );
}
