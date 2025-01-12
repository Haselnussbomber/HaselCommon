using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Dalamud.Game.Config;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using HaselCommon.Extensions.Collections;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Data.Files;
using Lumina.Extensions;

namespace HaselCommon.Services;

[RegisterSingleton]
public class TextureService(ITextureProvider textureProvider, IDataManager dataManager, IGameConfig gameConfig)
{
    private record UldPartKey(byte ThemeType, string UldName, uint PartListId, uint PartIndex);
    private record UldPartInfo(string Path, Vector2 Uv0, Vector2 Uv1);

    private static readonly string[] ThemePaths = ["", "light/", "third/", "fourth/"];
    private static readonly string[] GfdTextures = [
        "common/font/fonticon_xinput.tex",
        "common/font/fonticon_ps3.tex",
        "common/font/fonticon_ps4.tex",
        "common/font/fonticon_ps5.tex",
        "common/font/fonticon_lys.tex",
    ];

    private readonly ConcurrentDictionary<UldPartKey, UldPartInfo?> _uldPartCache = [];
    public readonly Lazy<GfdFileView> GfdFileView = new(() => new(dataManager.GetFile("common/font/gfdata.gfd")!.Data));

    public void Draw(string path, DrawInfo drawInfo)
    {
        if (drawInfo.DrawSize.HasValue && !ImGui.IsRectVisible(drawInfo.DrawSize.Value * (drawInfo.Scale ?? 1f)))
        {
            ImGui.Dummy(drawInfo.DrawSize.Value * (drawInfo.Scale ?? 1f));
            return;
        }

        var textureWrap = textureProvider.GetFromGame(path).GetWrapOrEmpty();
        Draw(textureWrap, drawInfo);
    }

    public void DrawIcon(GameIconLookup gameIconLookup, DrawInfo drawInfo)
    {
        if (drawInfo.DrawSize.HasValue && !ImGui.IsRectVisible(drawInfo.DrawSize.Value * (drawInfo.Scale ?? 1f)))
        {
            ImGui.Dummy(drawInfo.DrawSize.Value * (drawInfo.Scale ?? 1f));
            return;
        }

        if (!textureProvider.TryGetFromGameIcon(gameIconLookup, out var texture))
        {
            ImGui.Dummy(Vector2.Zero);
            return;
        }

        Draw(texture.GetWrapOrEmpty(), drawInfo);
    }

    public void DrawIcon(int iconId, bool isHq, DrawInfo drawInfo)
        => DrawIcon(new GameIconLookup((uint)iconId, isHq), drawInfo);

    public void DrawIcon(uint iconId, DrawInfo drawInfo)
        => DrawIcon(new GameIconLookup(iconId), drawInfo);

    public void DrawIcon(int iconId, DrawInfo drawInfo)
        => DrawIcon(new GameIconLookup((uint)iconId), drawInfo);

    public void DrawGfd(uint gfdIconId, DrawInfo drawInfo)
    {
        if (!GfdFileView.Value.TryGetEntry(gfdIconId, out var entry))
        {
            ImGui.Dummy((drawInfo.DrawSize ?? new(20)) * (drawInfo.Scale ?? 1f));
            return;
        }

        var startPos = new Vector2(entry.Left, entry.Top) * 2 + new Vector2(0, 340);
        var size = new Vector2(entry.Width, entry.Height) * 2;

        gameConfig.TryGet(SystemConfigOption.PadSelectButtonIcon, out uint padSelectButtonIcon);

        Draw(GfdTextures[padSelectButtonIcon], new()
        {
            DrawSize = (drawInfo.DrawSize ?? ImGuiHelpers.ScaledVector2(size.X, size.Y) / 2) * (drawInfo.Scale ?? 1f),
            Uv0 = startPos,
            Uv1 = startPos + size,
            TransformUv = true,
        });
    }

    public unsafe void DrawPart(string uldName, uint partListId, uint partIndex, DrawInfo drawInfo)
    {
        var themeType = RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType;
        var key = new UldPartKey(themeType, uldName, partListId, partIndex);

        if (_uldPartCache.TryGetValue(key, out var uldPartInfo))
        {
            if (uldPartInfo == null || uldPartInfo == null)
            {
                ImGui.Dummy((drawInfo.DrawSize ?? Vector2.Zero) * (drawInfo.Scale ?? 1f));
                return;
            }

            drawInfo.Uv0 = uldPartInfo.Uv0;
            drawInfo.Uv1 = uldPartInfo.Uv1;
            drawInfo.TransformUv = true;
            Draw(uldPartInfo.Path, drawInfo);
            return;
        }

        if (!TryGetUldPartInfo(key, out uldPartInfo))
        {
            ImGui.Dummy((drawInfo.DrawSize ?? Vector2.Zero) * (drawInfo.Scale ?? 1f));
            return;
        }

        drawInfo.Uv0 = uldPartInfo.Uv0;
        drawInfo.Uv1 = uldPartInfo.Uv1;
        drawInfo.TransformUv = true;

        Draw(uldPartInfo.Path, drawInfo);
    }

    private bool TryGetUldPartInfo(UldPartKey key, [NotNullWhen(returnValue: true)] out UldPartInfo? uldPartInfo)
    {
        uldPartInfo = null;

        var uld = dataManager.GetFile<UldFile>($"ui/uld/{key.UldName}.uld");
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

        var colorThemePath = ThemePaths[key.ThemeType];
        var assetPath = new string(asset.Path, 0, asset.Path.IndexOf('\0'));

        // check if theme high-res texture exists
        var path = assetPath;
        path = path.Insert(7, colorThemePath);
        path = path.Insert(path.LastIndexOf('.'), "_hr1");
        var exists = dataManager.FileExists(path);
        var version = 2;

        // fallback to theme normal texture
        if (!exists)
        {
            path = assetPath;
            path = path.Insert(7, colorThemePath);
            exists = dataManager.FileExists(path);
            version = 1;
        }

        // check if default theme high-res texture exists
        if (!exists)
        {
            path = assetPath;
            path = path.Insert(path.LastIndexOf('.'), "_hr1");
            exists = dataManager.FileExists(path);
            version = 2;
        }

        // fallback to default theme normal texture
        if (!exists)
        {
            path = assetPath;
            exists = dataManager.FileExists(path);
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

    public unsafe bool TryGetUldPartSize(string uldName, uint partListId, uint partIndex, out Vector2 size)
    {
        var themeType = RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType;

        if (!TryGetUldPartInfo(new(themeType, uldName, partListId, partIndex), out var uldPartInfo))
        {
            size = default;
            return false;
        }

        size = uldPartInfo.Uv1 - uldPartInfo.Uv0;
        return true;
    }

    public static void Draw(IDalamudTextureWrap? textureWrap, DrawInfo drawInfo)
    {
        if (textureWrap == null)
        {
            ImGui.Dummy((drawInfo.DrawSize ?? Vector2.Zero) * (drawInfo.Scale ?? 1f));
            return;
        }

        var size = (drawInfo.DrawSize ?? textureWrap.Size) * (drawInfo.Scale ?? 1f);
        var uv0 = drawInfo.Uv0 ?? Vector2.Zero;
        var uv1 = drawInfo.Uv1 ?? Vector2.One;

        if (drawInfo.TransformUv)
        {
            uv0 /= textureWrap.Size;
            uv1 /= textureWrap.Size;
        }

        ImGui.Image(
            textureWrap.ImGuiHandle,
            size,
            uv0,
            uv1,
            drawInfo.TintColor ?? Vector4.One,
            drawInfo.BorderColor ?? Vector4.Zero);
    }
}

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

    public static implicit operator DrawInfo(Vector2 size) => new(size);
    public static implicit operator DrawInfo(float size) => new(size);
    public static implicit operator DrawInfo(int size) => new(size);
}
