namespace HaselCommon.Extensions;

public static class Vector2Extensions
{
    extension(Vector2 value)
    {
        public Vector2 XOnly()
        {
            return new(value.X, 0);
        }

        public Vector2 YOnly()
        {
            return new(0, value.Y);
        }

        public Vector2 Cover(Vector2 maxSize)
        {
            var scaleFactorWidth = maxSize.X / value.X;
            var scaleFactorHeight = maxSize.Y / value.Y;

            var scaleFactor = MathF.Max(scaleFactorWidth, scaleFactorHeight);

            var scaledWidth = value.X * scaleFactor;
            var scaledHeight = value.Y * scaleFactor;

            return new(scaledWidth, scaledHeight);
        }

        public Vector2 Contain(Vector2 maxSize)
        {
            var scaleFactorWidth = maxSize.X / value.X;
            var scaleFactorHeight = maxSize.Y / value.Y;
            var scaleFactor = MathF.Min(scaleFactorWidth, scaleFactorHeight);

            return new(value.X * scaleFactor, value.Y * scaleFactor);
        }
    }
}
