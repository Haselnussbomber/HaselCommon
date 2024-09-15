namespace HaselCommon.Events;

#pragma warning disable CS9113
internal class DalamudEvents(
    ClientStateEventEmitter clientStateEventEmitter,
    ConditionEventEmitter conditionEventEmitter,
    GameObjectManager gameObjectManager
);
#pragma warning restore CS9113
