using Dalamud.Interface.Textures.TextureWraps;

namespace HaselCommon.Extensions;

public static class IDalamudTextureWrapExtensions
{
    extension(IDalamudTextureWrap textureWrap)
    {
        public void Draw(DrawInfo drawInfo)
        {
            var scale = drawInfo.Scale ?? 1f;
            var uv0 = drawInfo.Uv0 ?? Vector2.Zero;
            var uv1 = drawInfo.Uv1 ?? Vector2.One;

            var size = drawInfo.Fit switch
            {
                ContentFit.Cover => textureWrap.Size.Cover(drawInfo.DrawSize ?? new Vector2(ImGui.GetContentRegionAvail().X, 1)),
                ContentFit.Contain => textureWrap.Size.Contain(drawInfo.DrawSize ?? ImGui.GetContentRegionAvail()),
                _ => drawInfo.DrawSize ?? textureWrap.Size
            };

            if (drawInfo.TransformUv)
            {
                uv0 /= textureWrap.Size;
                uv1 /= textureWrap.Size;
            }

            ImGui.Image(
                textureWrap.Handle,
                size * scale,
                uv0,
                uv1,
                drawInfo.TintColor ?? Vector4.One,
                drawInfo.BorderColor ?? Vector4.Zero);
        }
    }
}
