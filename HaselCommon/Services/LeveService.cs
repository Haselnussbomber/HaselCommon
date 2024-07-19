using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Services;

public class LeveService(ExcelService ExcelService)
{
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
        return GetActiveLeveIds().Select(id => ExcelService.GetRow<Leve>(id)).Where(row => row != null).Cast<Leve>();
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
        => leve.LeveAssignmentType.Row is >= 5 and <= 12;

    public bool IsFishingLeve(Leve leve)
        => leve.LeveAssignmentType.Row is 4;
}
