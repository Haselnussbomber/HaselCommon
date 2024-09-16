using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace HaselCommon.Events;

public static class GameObjectEvents
{
    // GameObject
    public const string Created = "GameObject.Created";
    public const string Destroyed = "GameObject.Destroyed";
    public const string NameChanged = "GameObject.NameChanged";
    public const string PositionChanged = "GameObject.PositionChanged";
    public const string RotationChanged = "GameObject.RotationChanged";
    public const string IsDeadChanged = "GameObject.IsDeadChanged";
    public const string IsMountedChanged = "GameObject.IsMountedChanged";

    // Character
    public const string HealthChanged = "Character.HealthChanged";
    public const string MaxHealthChanged = "Character.MaxHealthChanged";
    public const string ManaChanged = "Character.ManaChanged";
    public const string MaxManaChanged = "Character.MaxManaChanged";
    public const string GatheringPointsChanged = "Character.GatheringPointsChanged";
    public const string MaxGatheringPointsChanged = "Character.MaxGatheringPointsChanged";
    public const string CraftingPointsChanged = "Character.CraftingPointsChanged";
    public const string MaxCraftingPointsChanged = "Character.MaxCraftingPointsChanged";
    public const string TitleIdChanged = "Character.TitleIdChanged";
    public const string ClassJobChanged = "Character.ClassJobChanged";
    public const string LevelChanged = "Character.LevelChanged";
    public const string ShieldValueChanged = "Character.ShieldValueChanged";
    public const string OnlineStatusChanged = "Character.OnlineStatusChanged";
    public const string IsWeaponDrawnChanged = "Character.IsWeaponDrawnChanged";
    public const string IsOffhandDrawnChanged = "Character.IsOffhandDrawnChanged";
    public const string InCombatChanged = "Character.InCombatChanged";
    public const string IsHostileChanged = "Character.IsHostileChanged";
    public const string StartedCasting = "Character.StartedCasting";
    public const string StoppedCasting = "Character.StoppedCasting";
    public const string IsPartyMemberChanged = "Character.IsPartyMemberChanged";
    public const string IsAllianceMemberChanged = "Character.IsAllianceMemberChanged";
    public const string IsFriendChanged = "Character.IsFriendChanged";

    public class GameObjectCreatedEventArgs : EventArgs
    {
        public required GameObjectId ObjectId { get; init; }
        public required nint Pointer { get; init; }
    }

    public class GameObjectDestroyedEventArgs : EventArgs
    {
        public required GameObjectId ObjectId { get; init; }
    }

    public class GameObjectChangedEventArgs : EventArgs
    {
        public required GameObjectId ObjectId { get; init; }
        public required nint Pointer { get; init; }
    }

    public class GameObjectChangedEventArgs<TValue> : ValueChangedEventArgs<TValue> where TValue : notnull
    {
        public required GameObjectId ObjectId { get; init; }
        public required nint Pointer { get; init; }
    }

    public class GameObjectStartedCastingEventArgs : GameObjectChangedEventArgs
    {
        public required ActionType ActionType { get; init; }
        public required uint ActionId { get; init; }
        public required GameObjectId TargetId { get; init; }
    }
}
