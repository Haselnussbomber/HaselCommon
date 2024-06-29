using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using HaselCommon.Extensions;
using Lumina.Data.Files;

namespace HaselCommon.Textures;

public class TextureManager : IDisposable
{
    private static readonly string[] ThemePaths = ["", "light/", "third/", "fourth/"];

    private readonly Dictionary<(string Path, int Version, Vector2? Uv0, Vector2? Uv1), Texture> _cache = [];
    private readonly Dictionary<(uint IconId, bool IsHq), Texture> _iconTexCache = [];
    private readonly Dictionary<(string UldName, uint PartListId, uint PartId), Texture> _uldTexCache = [];
    private readonly IFramework _framework;
    private readonly IDataManager _dataManager;
    private readonly ITextureProvider _textureProvider;

    public TextureManager(IFramework framework, IDataManager dataManager, ITextureProvider textureProvider)
    {
        _framework = framework;
        _dataManager = dataManager;
        _textureProvider = textureProvider;

        _framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;

        _iconTexCache.Clear();
        _uldTexCache.Clear();
        _cache.Dispose();

        GC.SuppressFinalize(this);
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        lock (_cache)
        {
            _iconTexCache.RemoveAll((key, value) => value.IsExpired);
            _uldTexCache.RemoveAll((key, value) => value.IsExpired);
            _cache.RemoveAll((key, value) => value.IsExpired, true);
        }
    }

    public Texture Get(string path, int version = 1, Vector2? uv0 = null, Vector2? uv1 = null)
    {
        var key = (path, version, uv0, uv1);

        if (!_cache.TryGetValue(key, out var tex))
        {
            lock (_cache)
                _cache.Add(key, tex = new(path, version, uv0, uv1));
        }

        return tex;
    }

    public Texture GetIcon(uint iconId, bool isHq = false)
    {
        var key = (iconId, isHq);

        if (_iconTexCache.TryGetValue(key, out var tex))
            return tex;

        if (!_textureProvider.TryGetIconPath(new GameIconLookup(iconId, isHq), out var path))
            return Get(Texture.EmptyIconPath);

        var version = path.EndsWith("_hr1.tex") ? 2 : 1;

        lock (_iconTexCache)
            _iconTexCache.Add(key, tex = Get(path, version));

        return tex;
    }

    public Texture GetIcon(int iconId)
        => GetIcon((uint)iconId);

    public unsafe Texture GetPart(string uldName, uint partListId, uint partIndex)
    {
        var key = (uldName, partListId, partIndex);

        if (_uldTexCache.TryGetValue(key, out var tex))
            return tex;

        var uld = _dataManager.GetFile<UldFile>($"ui/uld/{uldName}.uld");

        if (uld == null)
            return Get(Texture.EmptyIconPath);

        if (!uld.Parts.FindFirst((partList) => partList.Id == partListId, out var partList) || partList.PartCount < partIndex)
            return Get(Texture.EmptyIconPath);

        var part = partList.Parts[partIndex];

        if (!uld.AssetData.FindFirst((asset) => asset.Id == part.TextureId, out var asset))
            return Get(Texture.EmptyIconPath);

        var colorThemePath = ThemePaths[RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType];

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

        // fallback to transparent texture
        if (!exists)
            return Get(Texture.EmptyIconPath);

        var uv0 = new Vector2(part.U, part.V) * version;
        var uv1 = new Vector2(part.U + part.W, part.V + part.H) * version;

        lock (_uldTexCache)
            _uldTexCache.Add(key, tex = Get(path, version, uv0, uv1));

        return tex;
    }
}
