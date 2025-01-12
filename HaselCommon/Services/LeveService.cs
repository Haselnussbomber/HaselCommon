using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Extensions.Strings;
using HaselCommon.Utils;
using Lumina.Excel.Sheets;

namespace HaselCommon.Services;

[RegisterSingleton]
public class LeveService(ExcelService excelService, LanguageProvider languageProvider)
{
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
            if (excelService.TryGetRow<Leve>(leveId, out var leve))
                yield return leve;
        }
    }

    public int GetNumAcceptedLeveQuests()
        => GetActiveLeveIds().Count();

    public bool HasAcceptedLeveQuests()
        => GetActiveLeveIds().Any();

    public unsafe int GetNumLeveAllowances()
        => QuestManager.Instance()->NumLeveAllowances;

    public unsafe LeveWork* GetLeveWork(Leve leve)
    {
        var leveQuests = QuestManager.Instance()->LeveQuests;

        for (var i = 0; i < leveQuests.Length; i++)
        {
            if (leveQuests[i].LeveId == leve.RowId)
                return leveQuests.GetPointer(i);
        }

        return null;
    }

    public unsafe bool IsComplete(Leve leve)
        => QuestManager.Instance()->IsLevequestComplete((ushort)leve.RowId);

    public bool IsAccepted(Leve leve)
        => GetActiveLeveIds().Any(id => id == leve.RowId);

    public unsafe bool IsReadyForTurnIn(Leve leve)
    {
        var leveWork = GetLeveWork(leve);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 255;
    }

    public unsafe bool IsStarted(Leve leve)
    {
        var leveWork = GetLeveWork(leve);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 1 && leveWork->ClearClass != 0;
    }

    public unsafe bool IsFailed(Leve leve)
    {
        var leveWork = GetLeveWork(leve);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 3;
    }

    public bool IsTownLocked(Leve leve)
        => leve.RowId is 546 or 556 or 566;

    public bool IsCraftLeve(Leve leve)
        => leve.LeveAssignmentType.RowId is >= 5 and <= 12;

    public bool IsFishingLeve(Leve leve)
        => leve.LeveAssignmentType.RowId is 4;

    public ItemAmount[] GetRequiredItems(Leve leve)
    {
        if (_requiredItemsCache.TryGetValue(leve.RowId, out var requiredItems))
            return requiredItems;

        if (!(IsCraftLeve(leve) || IsFishingLeve(leve)) || !leve.DataId.TryGetValue<CraftLeve>(out var craftLeve))
        {
            _requiredItemsCache.Add(leve.RowId, requiredItems = []);
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

        _requiredItemsCache.Add(leve.RowId, requiredItems = [.. dict.Values]);
        return requiredItems;
    }

    public string GetLeveName(Leve leve)
    {
        var key = (leve.RowId, languageProvider.ClientLanguage);

        if (_leveNameCache.TryGetValue(key, out var name))
            return name;

        _leveNameCache.Add(key, name = leve.Name.ExtractText().StripSoftHypen());
        return name;
    }
}
