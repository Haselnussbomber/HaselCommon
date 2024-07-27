using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Config;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using HaselCommon.Extensions;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Data.Files;

namespace HaselCommon.Services;

public class TextureService(ITextureProvider TextureProvider, IDataManager DataManager, IGameConfig GameConfig)
{
    private static readonly string[] ThemePaths = ["", "light/", "third/", "fourth/"];
    private static readonly string[] GfdTextures = [
        "common/font/fonticon_xinput.tex",
        "common/font/fonticon_ps3.tex",
        "common/font/fonticon_ps4.tex",
        "common/font/fonticon_ps5.tex",
        "common/font/fonticon_lys.tex",
    ];

    private readonly Dictionary<(byte ThemeType, string UldName, uint PartListId, uint PartId), (string Path, Vector2 Uv0, Vector2 Uv1)?> UldPartCache = [];
    public readonly Lazy<GfdFileView> GfdFileView = new(() => new(DataManager.GetFile("common/font/gfdata.gfd")!.Data));

    public void Draw(string path, DrawInfo drawInfo)
    {
        var textureWrap = TextureProvider.GetFromGame(path).GetWrapOrEmpty();
        Draw(textureWrap, drawInfo);
    }

    public void DrawIcon(GameIconLookup gameIconLookup, DrawInfo drawInfo)
    {
        var textureWrap = TextureProvider.GetFromGameIcon(gameIconLookup).GetWrapOrEmpty();
        Draw(textureWrap, drawInfo);
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
            ImGui.Dummy(drawInfo.DrawSize ?? new(20));
            return;
        }

        var startPos = new Vector2(entry.Left, entry.Top) * 2 + new Vector2(0, 340);
        var size = new Vector2(entry.Width, entry.Height) * 2;

        GameConfig.TryGet(SystemConfigOption.PadSelectButtonIcon, out uint padSelectButtonIcon);

        Draw(GfdTextures[padSelectButtonIcon], new()
        {
            DrawSize = drawInfo.DrawSize ?? ImGuiHelpers.ScaledVector2(size.X, size.Y) / 2,
            Uv0 = startPos,
            Uv1 = startPos + size,
            TransformUv = true,
        });
    }

    public unsafe void DrawPart(string uldName, uint partListId, uint partIndex, DrawInfo drawInfo)
    {
        var themeType = RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType;
        var key = (themeType, uldName, partListId, partIndex);

        if (UldPartCache.TryGetValue(key, out var tuple))
        {
            if (tuple == null || !tuple.HasValue)
            {
                ImGui.Dummy(drawInfo.DrawSize ?? Vector2.Zero);
                return;
            }

            drawInfo.Uv0 = tuple.Value.Uv0;
            drawInfo.Uv1 = tuple.Value.Uv1;
            drawInfo.TransformUv = true;
            Draw(tuple.Value.Path, drawInfo);
            return;
        }

        var uld = DataManager.GetFile<UldFile>($"ui/uld/{uldName}.uld");

        if (uld == null)
        {
            lock (UldPartCache) UldPartCache.Add(key, null);
            ImGui.Dummy(drawInfo.DrawSize ?? Vector2.Zero);
            return;
        }

        if (!uld.Parts.FindFirst((partList) => partList.Id == partListId, out var partList) || partList.PartCount < partIndex)
        {
            lock (UldPartCache) UldPartCache.Add(key, null);
            ImGui.Dummy(drawInfo.DrawSize ?? Vector2.Zero);
            return;
        }

        var part = partList.Parts[partIndex];

        if (!uld.AssetData.FindFirst((asset) => asset.Id == part.TextureId, out var asset))
        {
            lock (UldPartCache) UldPartCache.Add(key, null);
            ImGui.Dummy(drawInfo.DrawSize ?? Vector2.Zero);
            return;
        }

        var colorThemePath = ThemePaths[themeType];

        var assetPath = new string(asset.Path, 0, asset.Path.IndexOf('\0'));

        // check if theme high-res texture exists
        var path = assetPath;
        path = path.Insert(7, colorThemePath);
        path = path.Insert(path.LastIndexOf('.'), "_hr1");
        var exists = DataManager.FileExists(path);
        var version = 2;

        // fallback to theme normal texture
        if (!exists)
        {
            path = assetPath;
            path = path.Insert(7, colorThemePath);
            exists = DataManager.FileExists(path);
            version = 1;
        }

        // check if default theme high-res texture exists
        if (!exists)
        {
            path = assetPath;
            path = path.Insert(path.LastIndexOf('.'), "_hr1");
            exists = DataManager.FileExists(path);
            version = 2;
        }

        // fallback to default theme normal texture
        if (!exists)
        {
            path = assetPath;
            exists = DataManager.FileExists(path);
            version = 1;
        }

        // fallback to dummy
        if (!exists)
        {
            lock (UldPartCache) UldPartCache.Add(key, null);
            ImGui.Dummy(drawInfo.DrawSize ?? Vector2.Zero);
            return;
        }

        var uv0 = new Vector2(part.U, part.V) * version;
        var uv1 = new Vector2(part.U + part.W, part.V + part.H) * version;

        lock (UldPartCache)
            UldPartCache.Add(key, (path, uv0, uv1));

        drawInfo.Uv0 = uv0;
        drawInfo.Uv1 = uv1;
        drawInfo.TransformUv = true;

        Draw(path, drawInfo);
    }

    public static void Draw(IDalamudTextureWrap? textureWrap, DrawInfo drawInfo)
    {
        if (textureWrap == null)
        {
            ImGui.Dummy(drawInfo.DrawSize ?? Vector2.Zero);
            return;
        }

        var size = drawInfo.DrawSize ?? textureWrap.Size;

        if (!ImGuiUtils.IsInViewport(size))
        {
            ImGui.Dummy(size);
            return;
        }

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
