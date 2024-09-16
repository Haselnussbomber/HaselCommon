using Dalamud.Game.ClientState.Conditions;

namespace HaselCommon.Events;

public static class ConditionEvents
{
    public const string Changed = "Player.ConditionChanged";

    public class ConditionChangedEventArgs : EventArgs
    {
        public required ConditionFlag Flag { get; init; }
        public required bool Value { get; init; }
    }
}
