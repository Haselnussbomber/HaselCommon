using System.Collections.Generic;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using HaselCommon.Services.Events;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;

namespace HaselCommon.Events;

internal class GameObjectManager : IDisposable
{
    private readonly IFramework _framework;
    private readonly Dictionary<ulong, Pointer<CSGameObject>> CurrentGameObjectIds = [];
    private readonly Dictionary<ulong, GameObject> GameObjectIdSorted = [];
    private readonly IEventEmitter _eventEmitter;

    public GameObjectManager(IEventEmitter eventEmitter, IFramework framework)
    {
        _eventEmitter = eventEmitter;
        _framework = framework;
        _framework.Update += Update;
    }

    public void Dispose()
    {
        _framework.Update -= Update;
        GameObjectIdSorted.Dispose();
        GC.SuppressFinalize(this);
    }

    private unsafe void Update(IFramework framework)
    {
        var gameObjectManager = CSGameObjectManager.Instance();

        CurrentGameObjectIds.Clear();

        for (var i = 0; i < gameObjectManager->Objects.GameObjectIdSortedCount; i++)
        {
            var gameObjectPointer = gameObjectManager->Objects.GameObjectIdSorted[i];
            if (gameObjectPointer.Value == null)
                continue;

            var gameObjectId = gameObjectPointer.Value->GetGameObjectId();

            if (!GameObjectIdSorted.TryGetValue(gameObjectId, out var gameObject))
            {
                GameObjectIdSorted.Add(gameObjectId, gameObject = new GameObject(_eventEmitter, gameObjectId));
                _eventEmitter.TriggerEvent(GameObjectEvents.Created, GameObjectEvents.GameObjectCreatedEventArgs.With(gameObject));
            }

            CurrentGameObjectIds.Add(gameObjectId, gameObjectPointer);
        }

        foreach (var (objectId, gameObject) in GameObjectIdSorted)
        {
            if (CurrentGameObjectIds.TryGetValue(objectId, out var gameObjectPointer))
                gameObject.Update(gameObjectPointer);
            else
            {
                _eventEmitter.TriggerEvent(GameObjectEvents.Destroyed, GameObjectEvents.GameObjectDestroyedEventArgs.With(objectId));
                GameObjectIdSorted.Remove(objectId);
            }
        }
    }
}
