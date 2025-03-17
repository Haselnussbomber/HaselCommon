namespace HaselCommon.Events;

public static class GameEvents
{
    public const string TerritoryChanged = $"{nameof(GameEvents)}.TerritoryChanged";

    public struct TerritoryChangedArgs
    {
        public uint TerritoryTypeId;
    }
}
