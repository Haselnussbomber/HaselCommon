namespace HaselCommon.Extensions;

public static class Vector3Extensions
{
    public static Vector3 Pow(this Vector3 vec, float exp)
    {
        return new Vector3(
            MathF.Pow(vec.X, exp),
            MathF.Pow(vec.Y, exp),
            MathF.Pow(vec.Z, exp)
        );
    }
}
