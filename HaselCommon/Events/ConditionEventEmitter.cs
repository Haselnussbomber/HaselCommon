using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using HaselCommon.Services.Events;

namespace HaselCommon.Events;

internal sealed class ConditionEventEmitter : IDisposable
{
    private readonly IEventEmitter _eventEmitter;
    private readonly ICondition _condition;

    public ConditionEventEmitter(IEventEmitter eventEmitter, ICondition condition)
    {
        _eventEmitter = eventEmitter;
        _condition = condition;

        _condition.ConditionChange += OnConditionChange;
    }

    public void Dispose()
    {
        _condition.ConditionChange -= OnConditionChange;
    }

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        _eventEmitter.TriggerEvent(ConditionEvents.Update, ConditionEvents.ConditionUpdateEventArgs.With(flag, value));
    }
}
