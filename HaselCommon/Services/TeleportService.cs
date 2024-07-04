using Dalamud.Plugin.Services;

namespace HaselCommon.Services;

public class TeleportService(IAetheryteList AetheryteList)
{
    public uint GetTeleportCost(uint destinationTerritoryTypeId)
    {
        var cost = 0u;

        foreach (var aetheryte in AetheryteList)
        {
            if (aetheryte.TerritoryId == destinationTerritoryTypeId && (cost == 0 || aetheryte.GilCost < cost))
            {
                cost = aetheryte.GilCost;
            }
        }

        return cost;
    }
}
