using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class PlayerService
{
    private readonly ExcelService _excelService;

    public bool IsLoggedIn => AgentLobby.Instance()->IsLoggedIn && PlayerState.Instance()->IsLoaded == 1;
    public uint CurrentClassJobId => PlayerState.Instance()->CurrentClassJobId;

    public bool TryGetClassJob(uint classJobId, out ClassJob classJob)
        => _excelService.TryGetRow(classJobId, out classJob);

    public bool TryGetClassJob(out ClassJob classJob)
        => TryGetClassJob(CurrentClassJobId, out classJob);

    public bool IsGatherer(uint? classJobId = null)
        => _excelService.TryGetRow<RawRow>("ClassJobCategory", 32, out var categoryRow) && categoryRow.ReadBool(4u + (classJobId ?? CurrentClassJobId));

    public bool IsCrafter(uint? classJobId = null)
        => _excelService.TryGetRow<RawRow>("ClassJobCategory", 33, out var categoryRow) && categoryRow.ReadBool(4u + (classJobId ?? CurrentClassJobId));
}
