namespace HaselCommon.Extensions;

public static class Vector2Extensions
{
    extension(Vector2 vec)
    {
        public Vector2 XOnly()
        {
            return new(vec.X, 0);
        }

        public Vector2 YOnly()
        {
            return new(0, vec.Y);
        }

        public Vector2 Cover(Vector2 maxSize)
        {
            var scaleFactorWidth = maxSize.X / vec.X;
            var scaleFactorHeight = maxSize.Y / vec.Y;

            var scaleFactor = MathF.Max(scaleFactorWidth, scaleFactorHeight);

            var scaledWidth = vec.X * scaleFactor;
            var scaledHeight = vec.Y * scaleFactor;

            return new(scaledWidth, scaledHeight);
        }

        public Vector2 Contain(Vector2 maxSize)
        {
            var scaleFactorWidth = maxSize.X / vec.X;
            var scaleFactorHeight = maxSize.Y / vec.Y;
            var scaleFactor = MathF.Min(scaleFactorWidth, scaleFactorHeight);

            return new(vec.X * scaleFactor, vec.Y * scaleFactor);
        }
    }
}
