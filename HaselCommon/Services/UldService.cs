using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Data.Files;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class UldService
{
    private readonly ITextureProvider _textureProvider;
    private readonly IDataManager _dataManager;
    private readonly IGameConfig _gameConfig;

    private record struct UldPartKey(byte ThemeType, string UldName, uint PartListId, uint PartIndex);
    private record UldPartInfo(string Path, Vector2 Uv0, Vector2 Uv1);

    private readonly ConcurrentDictionary<UldPartKey, UldPartInfo?> _uldPartCache = [];

    public unsafe void DrawPart(string uldName, uint partListId, uint partIndex, DrawInfo drawInfo)
    {
        if (!drawInfo.IsRectVisible)
        {
            ImGui.Dummy(drawInfo.ScaledDrawSize);
            return;
        }

        var themeType = RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType;
        var key = new UldPartKey(themeType, uldName, partListId, partIndex);

        if (!_uldPartCache.TryGetValue(key, out var uldPartInfo) || !TryGetUldPartInfo(key, out uldPartInfo))
        {
            ImGui.Dummy(drawInfo.ScaledDrawSize);
            return;
        }

        drawInfo.Uv0 = uldPartInfo.Uv0;
        drawInfo.Uv1 = uldPartInfo.Uv1;
        drawInfo.TransformUv = true;
        _textureProvider.Draw(uldPartInfo.Path, drawInfo);
    }

    private bool TryGetUldPartInfo(UldPartKey key, [NotNullWhen(returnValue: true)] out UldPartInfo? uldPartInfo)
    {
        uldPartInfo = null;

        var uld = _dataManager.GetFile<UldFile>($"ui/uld/{key.UldName}.uld");
        if (uld == null)
        {
            _uldPartCache.TryAdd(key, null);
            return false;
        }

        if (!uld.Parts.TryGetFirst((partList) => partList.Id == key.PartListId, out var partList) || partList.PartCount < key.PartIndex)
        {
            _uldPartCache.TryAdd(key, null);
            return false;
        }

        var part = partList.Parts[key.PartIndex];

        if (!uld.AssetData.TryGetFirst((asset) => asset.Id == part.TextureId, out var asset))
        {
            _uldPartCache.TryAdd(key, null);
            return false;
        }

        var colorThemePath = key.ThemeType switch
        {
            not 0 => $"img{key.ThemeType:00}/",
            _ => string.Empty
        };
        var assetPath = new string(asset.Path, 0, asset.Path.IndexOf('\0'));

        // check if theme high-res texture exists
        var path = assetPath;
        path = path.Insert(7, colorThemePath);
        path = path.Insert(path.LastIndexOf('.'), "_hr1");
        var exists = _dataManager.FileExists(path);
        var version = 2;

        // fallback to theme normal texture
        if (!exists)
        {
            path = assetPath;
            path = path.Insert(7, colorThemePath);
            exists = _dataManager.FileExists(path);
            version = 1;
        }

        // check if default theme high-res texture exists
        if (!exists)
        {
            path = assetPath;
            path = path.Insert(path.LastIndexOf('.'), "_hr1");
            exists = _dataManager.FileExists(path);
            version = 2;
        }

        // fallback to default theme normal texture
        if (!exists)
        {
            path = assetPath;
            exists = _dataManager.FileExists(path);
            version = 1;
        }

        // fallback to dummy
        if (!exists)
        {
            _uldPartCache.TryAdd(key, null);
            return false;
        }

        var uv0 = new Vector2(part.U, part.V) * version;
        var uv1 = new Vector2(part.U + part.W, part.V + part.H) * version;

        uldPartInfo = new(path, uv0, uv1);
        _uldPartCache.TryAdd(key, uldPartInfo);

        return true;
    }
}
