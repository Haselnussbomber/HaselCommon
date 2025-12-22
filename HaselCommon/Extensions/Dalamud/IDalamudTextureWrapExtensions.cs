using Dalamud.Interface.Textures.TextureWraps;

namespace HaselCommon.Extensions;

public static class IDalamudTextureWrapExtensions
{
    extension(IDalamudTextureWrap textureWrap)
    {
        public void Draw(DrawInfo drawInfo)
        {
            var scale = drawInfo.Scale ?? 1f;
            var size = drawInfo.DrawSize ?? textureWrap.Size;
            var uv0 = drawInfo.Uv0 ?? Vector2.Zero;
            var uv1 = drawInfo.Uv1 ?? Vector2.One;

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
