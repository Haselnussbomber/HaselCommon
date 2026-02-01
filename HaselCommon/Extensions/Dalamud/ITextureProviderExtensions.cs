using Dalamud.Interface.Textures;

namespace HaselCommon.Extensions;

public static class ITextureProviderExtensions
{
    extension(ITextureProvider textureProvider)
    {
        public void Draw(string path, DrawInfo drawInfo)
        {
            if (drawInfo.IsRectVisible
                && textureProvider.GetFromGame(path).TryGetWrap(out var textureWrap, out _))
            {
                textureWrap.Draw(drawInfo);
            }
            else
            {
                ImGui.Dummy(drawInfo.ScaledDrawSize);
            }
        }

        public void DrawIcon(GameIconLookup gameIconLookup, DrawInfo drawInfo)
        {
            if (drawInfo.IsRectVisible
                && textureProvider.TryGetFromGameIcon(gameIconLookup, out var texture)
                && texture.TryGetWrap(out var textureWrap, out _))
            {
                textureWrap.Draw(drawInfo);
            }
            else
            {
                ImGui.Dummy(drawInfo.ScaledDrawSize);
            }
        }
    }
}
