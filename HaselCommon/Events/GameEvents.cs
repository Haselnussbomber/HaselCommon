namespace HaselCommon.Events;

#pragma warning disable CS9113
internal class GameEvents(
    ClientStateEventEmitter clientStateEventEmitter,
    ConditionEventEmitter conditionEventEmitter,
    GameObjectManager gameObjectManager,
    PlayerStateEventEmitter playerStateEventEmitter
);
#pragma warning restore CS9113
