namespace HaselCommon.Extensions;

public static class FishingSpotExtensions
{
    extension(FishingSpot row)
    {
        public uint FishingSpotIcon => !row.Rare ? 60465u : 60466u;
    }
}
