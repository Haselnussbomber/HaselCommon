using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Plugin;

namespace HaselCommon.ImGuiYoga;

public partial class Node
{
    private IFontHandle? _fontHandle;
    public IFontHandle? FontHandle => _fontHandle ?? Parent?.FontHandle;

    public void UpdateFontHandle()
    {
        var fontAtlas = Service.Get<IDalamudPluginInterface>().UiBuilder.FontAtlas;
        var size = ComputedStyle.FontSize;
        var fontStyle = ComputedStyle.FontFamily.ToLowerInvariant() switch
        {
            "jupiter" => new GameFontStyle(GameFontFamily.Jupiter, size),
            "jupiternumeric" => new GameFontStyle(GameFontFamily.JupiterNumeric, size),
            "meidinger" => new GameFontStyle(GameFontFamily.Meidinger, size),
            "miedingermid" => new GameFontStyle(GameFontFamily.MiedingerMid, size),
            "trumpgothic" => new GameFontStyle(GameFontFamily.TrumpGothic, size),
            _ => new GameFontStyle(GameFontFamily.Axis, size),
        };

        DisposeFontHandle();
        _fontHandle = fontAtlas.NewGameFontHandle(fontStyle);
        _fontHandle.ImFontChanged += FontHandle_ImFontChanged;
    }

    private void FontHandle_ImFontChanged(IFontHandle fontHandle, ILockedImFont lockedFont)
    {
        if (NodeType == YogaSharp.NodeType.Text || (HasMeasureFunc && Count == 0))
        {
            IsDirty = true;
        }

        Traverse((node) =>
        {
            if (node.NodeType == YogaSharp.NodeType.Text || (node.HasMeasureFunc && node.Count == 0))
            {
                node.IsDirty = true;
            }
        });
    }

    private void DisposeFontHandle()
    {
        if (_fontHandle != null)
        {
            _fontHandle.ImFontChanged -= FontHandle_ImFontChanged;
            _fontHandle.Dispose();
        }
    }
}
