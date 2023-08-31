using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedLeve : Leve
{
    private string? _name = null;
    private string? _levemeteName = null;
    private string? _townName = null;

    public unsafe LeveWork* LeveWork
        => QuestManager.Instance()->GetLeveQuestById((ushort)RowId);

    public new string Name
        => _name ??= base.Name?.ToDalamudString().ToString() ?? $"<{RowId}>";

    public string? LevemeteName
    {
        get
        {
            if (string.IsNullOrEmpty(_levemeteName) && LevelLevemete.Value?.Type == 8) // Type 8 = NPC?!?
                _levemeteName = GetENpcResidentName(LevelLevemete.Value.Object);

            return _levemeteName ?? "";
        }
    }

    public int TypeIcon
        => LeveAssignmentType.Value?.Icon ?? 0;

    public string TypeName
        => LeveAssignmentType.Value?.Name ?? "";

    public string TownName
        => _townName ??= Town.Value?.Name.ToDalamudString().ToString() ?? "???";

    public bool TownLocked
        => RowId == 546 || RowId == 556 || RowId == 566;

    public unsafe bool IsComplete
        => QuestManager.Instance()->IsLevequestComplete((ushort)RowId);

    public unsafe bool IsAccepted
        => LeveWork != null;

    public unsafe bool IsReadyForTurnIn
        => IsAccepted && LeveWork->Sequence == 255;

    public unsafe bool IsStarted
        => IsAccepted && LeveWork->Sequence == 1 && LeveWork->ClearClass != 0;

    public unsafe bool IsFailed
        => IsAccepted && LeveWork->Sequence == 3;

    public bool IsCraftLeve
        => LeveAssignmentType.Row is >= 5 and <= 12;
}
