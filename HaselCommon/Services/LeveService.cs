using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class LeveService
{
    private readonly ExcelService _excelService;

    private readonly Dictionary<(uint, ClientLanguage), string> _leveNameCache = [];
    private readonly Dictionary<uint, ItemAmount[]> _requiredItemsCache = [];
    
    public unsafe ValueEnumerable<Distinct<WhereSelect<FromSpan<LeveWork>, LeveWork, ushort>, ushort>, ushort> GetActiveLeveIds()
    {
        return QuestManager.Instance()->LeveQuests
            .AsValueEnumerable()
            .Where(entry => entry.LeveId != 0)
            .Select(entry => entry.LeveId)
            .Distinct();
    }

    public unsafe ValueEnumerable<Select<Distinct<WhereSelect<FromSpan<LeveWork>, LeveWork, ushort>, ushort>, ushort, Leve>, Leve> GetActiveLeves()
    {
        var leveSheet = _excelService.GetSheet<Leve>();

        return QuestManager.Instance()->LeveQuests
            .AsValueEnumerable()
            .Where(entry => entry.LeveId != 0)
            .Select(entry => entry.LeveId)
            .Distinct()
            .Select(leveId => leveSheet.GetRow(leveId));
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

    public unsafe LeveWork* GetLeveWork(uint leveId)
    {
        var leveQuests = QuestManager.Instance()->LeveQuests;

        for (var i = 0; i < leveQuests.Length; i++)
        {
            if (leveQuests[i].LeveId == leveId)
                return leveQuests.GetPointer(i);
        }

        return null;
    }

    public unsafe bool IsComplete(uint leveId)
    {
        return QuestManager.Instance()->IsLevequestComplete((ushort)leveId);
    }

    public bool IsAccepted(uint leveId)
    {
        return GetActiveLeveIds().Any(id => id == leveId);
    }

    public unsafe bool IsReadyForTurnIn(uint leveId)
    {
        var leveWork = GetLeveWork(leveId);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 255;
    }

    public unsafe bool IsStarted(uint leveId)
    {
        var leveWork = GetLeveWork(leveId);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 1 && leveWork->ClearClass != 0;
    }

    public unsafe bool IsFailed(uint leveId)
    {
        var leveWork = GetLeveWork(leveId);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 3;
    }

    public bool IsTownLocked(uint leveId)
    {
        return leveId is 546 or 556 or 566;
    }

    public bool IsCraftLeve(uint leveId)
    {
        return _excelService.TryGetRow<Leve>(leveId, out var leve) && leve.LeveAssignmentType.RowId is >= 5 and <= 12;
    }

    public bool IsFishingLeve(uint leveId)
    {
        return _excelService.TryGetRow<Leve>(leveId, out var leve) && leve.LeveAssignmentType.RowId is 4;
    }

    public ItemAmount[] GetRequiredItems(uint leveId)
    {
        if (_requiredItemsCache.TryGetValue(leveId, out var requiredItems))
            return requiredItems;

        if (!(IsCraftLeve(leveId) || IsFishingLeve(leveId)) || !_excelService.TryGetRow<Leve>(leveId, out var leve) || !leve.DataId.TryGetValue<CraftLeve>(out var craftLeve))
        {
            _requiredItemsCache.Add(leveId, requiredItems = []);
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

        _requiredItemsCache.Add(leveId, requiredItems = [.. dict.Values]);
        return requiredItems;
    }
}
