using Dalamud.Game.ClientState.Conditions;

namespace HaselCommon.Events;

public static class ConditionEvents
{
    public const string Update = "Condition.Update";

    public class ConditionUpdateEventArgs : EventArgs
    {
        private static readonly ConditionUpdateEventArgs Instance = new();
        private ConditionUpdateEventArgs() { }

        public ConditionFlag Flag { get; private set; }
        public bool Value { get; private set; }

        internal static ConditionUpdateEventArgs With(ConditionFlag flag, bool value)
        {
            Instance.Flag = flag;
            Instance.Value = value;
            return Instance;
        }
    }
}
