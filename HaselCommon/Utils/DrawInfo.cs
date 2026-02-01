namespace HaselCommon.Utils;

public struct DrawInfo
{
    public DrawInfo()
    {
    }

    public DrawInfo(Vector2 size)
    {
        DrawSize = size;
    }

    public DrawInfo(float size)
    {
        DrawSize = new(size);
    }

    public DrawInfo(float width, float height)
    {
        DrawSize = new(width, height);
    }

    public float? Scale { get; set; }
    public Vector2? DrawSize { get; set; }
    public Vector2? Uv0 { get; set; }
    public Vector2? Uv1 { get; set; }
    public Vector4? TintColor { get; set; }
    public Vector4? BorderColor { get; set; }
    public bool TransformUv { get; set; }
    public ContentFit Fit { get; set; } = ContentFit.None;

    public Vector2 ScaledDrawSize => (DrawSize ?? Vector2.Zero) * (Scale ?? 1f);
    public bool IsRectVisible => !DrawSize.HasValue || (DrawSize.HasValue && ImGui.IsRectVisible(ScaledDrawSize));

    public static implicit operator DrawInfo(Vector2 size) => new(size);
    public static implicit operator DrawInfo(float size) => new(size);
    public static implicit operator DrawInfo(int size) => new(size);
}

public enum ContentFit
{
    None,
    Cover,
    Contain
}
