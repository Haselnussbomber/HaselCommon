using Dalamud.Game.Config;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class GfdService
{
    private readonly ITextureProvider _textureProvider;
    private readonly IDataManager _dataManager;
    private readonly IGameConfig _gameConfig;

    private static readonly string[] GfdTextures = [
        "common/font/fonticon_xinput.tex",
        "common/font/fonticon_ps3.tex",
        "common/font/fonticon_ps4.tex",
        "common/font/fonticon_ps5.tex",
        "common/font/fonticon_lys.tex",
    ];

    private Lazy<GfdFileView> _gfdFileView;

    [AutoPostConstruct]
    private void Initialize()
    {
        _gfdFileView = new(() => new(_dataManager.GetFile("common/font/gfdata.gfd")!.Data));
    }

    public ReadOnlySpan<GfdFileView.GfdEntry> Entries => _gfdFileView.Value.Entries;

    public void Draw(uint gfdIconId, DrawInfo drawInfo)
    {
        if (!_gfdFileView.Value.TryGetEntry(gfdIconId, out var entry))
        {
            ImGui.Dummy((drawInfo.DrawSize ?? new(20)) * (drawInfo.Scale ?? 1f));
            return;
        }

        var startPos = new Vector2(entry.Left, entry.Top) * 2 + new Vector2(0, 340);
        var size = new Vector2(entry.Width, entry.Height) * 2;

        _gameConfig.TryGet(SystemConfigOption.PadSelectButtonIcon, out uint padSelectButtonIcon);

        _textureProvider.Draw(GfdTextures[padSelectButtonIcon], new()
        {
            DrawSize = (drawInfo.DrawSize ?? ImGuiHelpers.ScaledVector2(size.X, size.Y) / 2) * (drawInfo.Scale ?? 1f),
            Uv0 = startPos,
            Uv1 = startPos + size,
            TransformUv = true,
        });
    }
}
