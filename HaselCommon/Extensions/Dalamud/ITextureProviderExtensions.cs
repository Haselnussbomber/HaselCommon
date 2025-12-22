using Dalamud.Interface.Textures;
using HaselCommon.Extensions.Dalamud;
using Lumina.Data.Files;

namespace HaselCommon.Extensions;

public static class ITextureProviderExtensions
{
    private static readonly Dictionary<uint, Vector2> IconSizeCache = [];

    extension(ITextureProvider textureProvider)
    {
        public bool TryGetIconSize(uint iconId, out Vector2 size)
        {
            if (IconSizeCache.TryGetValue(iconId, out size))
                return size != Vector2.Zero;

            var iconPath = textureProvider.GetIconPath(iconId);
            if (string.IsNullOrEmpty(iconPath))
            {
                size = Vector2.Zero;
                IconSizeCache[iconId] = size;
                return false;
            }

            if (!ServiceLocator.TryGetService<IDataManager>(out var dataManager)
                || !dataManager.TryGetFile<TexFile>(iconPath, out var file))
            {
                size = Vector2.Zero;
                IconSizeCache[iconId] = size;
                return false;
            }

            size = new Vector2(file.Header.Width, file.Header.Height);
            IconSizeCache[iconId] = size;
            return size != Vector2.Zero;
        }

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
