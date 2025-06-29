namespace HaselCommon.Extensions;

public static class FishingSpotExtensions
{
    extension(FishingSpot row)
    {
        public uint GetFishingSpotIcon() => !row.Rare ? 60465u : 60466u;
    }
}
