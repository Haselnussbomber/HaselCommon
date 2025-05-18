namespace HaselCommon.Extensions;

public static class Vector2Extensions
{
    public static Vector2 Cover(this Vector2 elementSize, Vector2 maxSize)
    {
        var scaleFactorWidth = maxSize.X / elementSize.X;
        var scaleFactorHeight = maxSize.Y / elementSize.Y;

        var scaleFactor = MathF.Max(scaleFactorWidth, scaleFactorHeight);

        var scaledWidth = elementSize.X * scaleFactor;
        var scaledHeight = elementSize.Y * scaleFactor;

        return new(scaledWidth, scaledHeight);
    }

    public static Vector2 Contain(this Vector2 elementSize, Vector2 maxSize)
    {
        var scaleFactorWidth = maxSize.X / elementSize.X;
        var scaleFactorHeight = maxSize.Y / elementSize.Y;
        var scaleFactor = MathF.Min(scaleFactorWidth, scaleFactorHeight);

        return new(elementSize.X * scaleFactor, elementSize.Y * scaleFactor);
    }
}
