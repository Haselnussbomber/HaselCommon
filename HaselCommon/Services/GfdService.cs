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

    private GfdFile _gfdFile;

    [AutoPostConstruct]
    private void Initialize()
    {
        _gfdFile = _dataManager.GetFile<GfdFile>("common/font/gfdata.gfd")!;
    }

    public ReadOnlySpan<GfdFile.GfdEntry> Entries => _gfdFile?.Entries ?? default;

    public void Draw(uint gfdIconId, DrawInfo drawInfo)
    {
        if (!drawInfo.IsRectVisible || !_gfdFile.TryGetEntry(gfdIconId, out var entry) || entry.IsEmpty)
        {
            ImGui.Dummy((drawInfo.DrawSize ?? new(20)) * (drawInfo.Scale ?? 1f));
            return;
        }

        var size = entry.CalculateScaledSize((drawInfo.DrawSize?.Y ?? 20) * (drawInfo.Scale ?? 1f), out var useHq);

        _gameConfig.TryGet(SystemConfigOption.PadSelectButtonIcon, out uint padSelectButtonIcon);

        _textureProvider.Draw(GfdTextures[padSelectButtonIcon], new()
        {
            DrawSize = size,
            Uv0 = useHq ? entry.HqUv0 : entry.Uv0,
            Uv1 = useHq ? entry.HqUv1 : entry.Uv1,
        });
    }
}
