using Dalamud.Interface.Textures;

namespace HaselCommon.Extensions;

public static class ITextureProviderExtensions
{
    public static void Draw(this ITextureProvider textureProvider, string path, DrawInfo drawInfo)
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

    public static void DrawIcon(this ITextureProvider textureProvider, GameIconLookup gameIconLookup, DrawInfo drawInfo)
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
