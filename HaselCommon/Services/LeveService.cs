using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Utils;
using Lumina.Excel.Sheets;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class LeveService
{
    private readonly ExcelService _excelService;

    private readonly Dictionary<(uint, ClientLanguage), string> _leveNameCache = [];
    private readonly Dictionary<uint, ItemAmount[]> _requiredItemsCache = [];

    public unsafe IEnumerable<ushort> GetActiveLeveIds()
    {
        var leveIds = new HashSet<ushort>();

        foreach (ref var entry in QuestManager.Instance()->LeveQuests)
        {
            if (entry.LeveId != 0)
                leveIds.Add(entry.LeveId);
        }

        return leveIds;
    }

    public IEnumerable<Leve> GetActiveLeves()
    {
        foreach (var leveId in GetActiveLeveIds())
        {
            if (_excelService.TryGetRow<Leve>(leveId, out var leve))
                yield return leve;
        }
    }

    public int GetNumAcceptedLeveQuests()
    {
        return GetActiveLeveIds().Count();
    }

    public bool HasAcceptedLeveQuests()
    {
        return GetActiveLeveIds().Any();
    }

    public unsafe int GetNumLeveAllowances()
    {
        return QuestManager.Instance()->NumLeveAllowances;
    }

    public unsafe LeveWork* GetLeveWork(ExcelRowId<Leve> leveId)
    {
        var leveQuests = QuestManager.Instance()->LeveQuests;

        for (var i = 0; i < leveQuests.Length; i++)
        {
            if (leveQuests[i].LeveId == leveId.RowId)
                return leveQuests.GetPointer(i);
        }

        return null;
    }

    public unsafe bool IsComplete(ExcelRowId<Leve> leveId)
    {
        return QuestManager.Instance()->IsLevequestComplete((ushort)leveId.RowId);
    }

    public bool IsAccepted(ExcelRowId<Leve> leveId)
    {
        return GetActiveLeveIds().Any(id => id == leveId.RowId);
    }

    public unsafe bool IsReadyForTurnIn(ExcelRowId<Leve> leveId)
    {
        var leveWork = GetLeveWork(leveId);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 255;
    }

    public unsafe bool IsStarted(ExcelRowId<Leve> leveId)
    {
        var leveWork = GetLeveWork(leveId);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 1 && leveWork->ClearClass != 0;
    }

    public unsafe bool IsFailed(ExcelRowId<Leve> leveId)
    {
        var leveWork = GetLeveWork(leveId);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 3;
    }

    public bool IsTownLocked(ExcelRowId<Leve> leveId)
    {
        return leveId.RowId is 546 or 556 or 566;
    }

    public bool IsCraftLeve(ExcelRowId<Leve> leveId)
    {
        return leveId.TryGetRow(out var leve) && leve.LeveAssignmentType.RowId is >= 5 and <= 12;
    }

    public bool IsFishingLeve(ExcelRowId<Leve> leveId)
    {
        return leveId.TryGetRow(out var leve) && leve.LeveAssignmentType.RowId is 4;
    }

    public ItemAmount[] GetRequiredItems(ExcelRowId<Leve> leveId)
    {
        if (_requiredItemsCache.TryGetValue(leveId.RowId, out var requiredItems))
            return requiredItems;

        if (!(IsCraftLeve(leveId) || IsFishingLeve(leveId)) || !leveId.TryGetRow(out var leve) || !leve.DataId.TryGetValue<CraftLeve>(out var craftLeve))
        {
            _requiredItemsCache.Add(leveId.RowId, requiredItems = []);
            return requiredItems;
        }

        var dict = new Dictionary<uint, ItemAmount>();

        for (var i = 0; i < craftLeve.Item.Count; i++)
        {
            var item = craftLeve.Item[i];
            var count = craftLeve.ItemCount[i];

            if (!item.IsValid || count == 0)
                continue;

            if (!dict.TryGetValue(item.RowId, out var reqItem))
                dict.Add(item.RowId, reqItem = new(item.Value, 0));

            reqItem.Amount += count;
        }

        _requiredItemsCache.Add(leveId.RowId, requiredItems = [.. dict.Values]);
        return requiredItems;
    }
}
