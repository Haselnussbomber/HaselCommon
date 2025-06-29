namespace HaselCommon.Extensions;

public static class HalfExtensions
{
    extension(Half value)
    {
        public bool IsApproximately(Half other)
        {
            return ((float)value).IsApproximately((float)other, 0.001f);
        }
    }
}
