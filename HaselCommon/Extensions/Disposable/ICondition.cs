using Dalamud.Game.ClientState.Conditions;

namespace HaselCommon.Extensions;

public static partial class IConditionExtensions
{
    public delegate void ConditionChangeDelegate(ConditionFlag flag, bool value);

    extension(ICondition condition)
    {
        public IDisposable OnConditionChange(ConditionChangeDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => condition.ConditionChange += handler,
                handler => condition.ConditionChange -= handler,
                (ICondition.ConditionChangeDelegate)handler.Invoke
            );
        }
    }
}
