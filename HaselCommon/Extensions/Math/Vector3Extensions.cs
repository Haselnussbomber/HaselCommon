namespace HaselCommon.Extensions;

public static class Vector3Extensions
{
    extension(Vector3 vec)
    {
        public Vector3 Pow(float exp)
        {
            return new Vector3(
                MathF.Pow(vec.X, exp),
                MathF.Pow(vec.Y, exp),
                MathF.Pow(vec.Z, exp)
            );
        }
    }
}
