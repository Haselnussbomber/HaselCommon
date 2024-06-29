using System.Linq;
using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using HaselCommon.Utils;
using ImGuiNET;

namespace HaselCommon.Textures;

public record Texture
{
    public static readonly string EmptyIconPath = "ui/icon/000000/000000.tex";
    private static readonly TimeSpan KeepAliveTime = TimeSpan.FromSeconds(2);

    private ISharedImmediateTexture? SharedTexture;
    private bool SizesSet;

    public Texture(string path, int version, Vector2? uv0 = null, Vector2? uv1 = null)
    {
        Path = path;
        Version = version;
        Uv0 = uv0;
        Uv1 = uv1;
    }

    public void Dispose()
    {
        TextureWrap?.Dispose();
        GC.SuppressFinalize(this);
    }

    public string Path { get; }
    public int Version { get; }
    public Vector2 Size { get; private set; }
    public Vector2? Uv0 { get; private set; }
    public Vector2? Uv1 { get; private set; }

    public IDalamudTextureWrap? TextureWrap { get; private set; }
    public DateTime LastAccess { get; private set; } = DateTime.UtcNow;
    public DateTime LastRender { get; private set; } = DateTime.MinValue;

    public bool IsExpired => LastAccess < DateTime.UtcNow - KeepAliveTime;

    public void Draw(Vector2? drawSize = null, Vector4? tintColor = null, Vector4? borderColor = null)
    {
        var size = drawSize ?? Size;

        LastAccess = DateTime.UtcNow;

        if (!ImGuiUtils.IsInViewport(size) || Path == EmptyIconPath)
        {
            ImGui.Dummy(size);

            if (SharedTexture != null && LastRender < DateTime.UtcNow - KeepAliveTime)
                SharedTexture = null;

            return;
        }

        SharedTexture ??= LoadTexture();
        LastRender = DateTime.UtcNow;

        if (SharedTexture == null || !SharedTexture.TryGetWrap(out var textureWrap, out var _))
        {
            ImGui.Dummy(size);
            return;
        }

        ImGui.Image(textureWrap.ImGuiHandle, size, Uv0 ?? Vector2.Zero, Uv1 ?? Vector2.One, tintColor ?? Vector4.One, borderColor ?? Vector4.Zero);
    }

    public void DrawRotated(float angle, Vector2? drawSize = null, Vector4? tintColor = null, bool noDummy = false)
    {
        var size = drawSize ?? Size;

        var rotationMatrix = Matrix3x2.CreateRotation(angle);
        var corners = new[] {
            -size, // top left
            new Vector2(size.X, -size.Y), // top right
            size, // bottom right
            new Vector2(-size.X, size.Y), // bottom left
        };

        for (var i = 0; i < corners.Length; i++)
            corners[i] = Vector2.Transform(corners[i], rotationMatrix);

        var xPositions = corners.Select(v => v.X);
        var yPositions = corners.Select(v => v.Y);
        var boxSize = new Vector2(
            xPositions.Max() - xPositions.Min(),
            yPositions.Max() - yPositions.Min()
        );

        LastAccess = DateTime.UtcNow;

        if (!ImGuiUtils.IsInViewport(boxSize) || Path == EmptyIconPath)
        {
            if (!noDummy)
                ImGui.Dummy(boxSize);

            if (SharedTexture != null && LastRender < DateTime.UtcNow - KeepAliveTime)
                SharedTexture = null;

            return;
        }

        SharedTexture ??= LoadTexture();
        LastRender = DateTime.UtcNow;

        if (SharedTexture != null && SharedTexture.TryGetWrap(out var textureWrap, out var _))
        {
            var position = ImGui.GetCursorPos() + size / 2;

            var uvTopLeft = Uv0 ?? Vector2.Zero;
            var uvBottomRight = Uv1 ?? Vector2.One;
            var uvTopRight = new Vector2(uvBottomRight.X, uvTopLeft.Y);
            var uvBottomLeft = new Vector2(uvTopLeft.X, uvBottomRight.Y);

            // Draw the image quad
            ImGui.GetWindowDrawList().AddImageQuad(
                textureWrap.ImGuiHandle,
                position + corners[0],
                position + corners[1],
                position + corners[2],
                position + corners[3],
                uvTopLeft,
                uvTopRight,
                uvBottomRight,
                uvBottomLeft,
                ImGui.GetColorU32(tintColor ?? Vector4.One));
        }

        if (!noDummy)
            ImGui.Dummy(boxSize);
    }

    public void Draw(float x, float y, Vector4? tintColor = null, Vector4? borderColor = null)
        => Draw(new Vector2(x, y), tintColor, borderColor);

    public void Draw(float dimensions, Vector4? tintColor = null, Vector4? borderColor = null)
        => Draw(new Vector2(dimensions), tintColor, borderColor);

    private ISharedImmediateTexture LoadTexture()
    {
        var tex = Service.Get<ITextureProvider>().GetFromGame(Path);

        if (tex.TryGetWrap(out var texture, out var _) && !SizesSet)
        {
            var texSize = new Vector2(texture.Width, texture.Height);

            // defaults
            Uv0 ??= Vector2.Zero;
            Uv1 ??= texSize;

            // set size depending on uv dimensions
            Size = Uv1.Value - Uv0.Value;

            // convert uv coordinates range from [[0, 0], [Width, Height]] to [[0, 0], [1, 1]] for ImGui
            Uv0 = Uv0.Value / texSize;
            Uv1 = Uv1.Value / texSize;

            SizesSet = true;
        }

        return tex;
    }
}
