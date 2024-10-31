using Lumina.Excel.Sheets;

namespace HaselCommon.Extensions.Sheets;

public static class FishingSpotExtensions
{
    public static uint GetFishingSpotIcon(this FishingSpot fishingSpot)
        => !fishingSpot.Rare ? 60465u : 60466u;
}
