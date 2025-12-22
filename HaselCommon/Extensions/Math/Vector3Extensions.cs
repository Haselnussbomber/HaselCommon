namespace HaselCommon.Extensions;

public static class Vector3Extensions
{
    extension(Vector3 value)
    {
        public Vector3 Pow(float exp)
        {
            return new Vector3(
                MathF.Pow(value.X, exp),
                MathF.Pow(value.Y, exp),
                MathF.Pow(value.Z, exp)
            );
        }
    }
}
