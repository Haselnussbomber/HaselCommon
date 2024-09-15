using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using HaselCommon.Services.Events;
using Lumina.Text.ReadOnly;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace HaselCommon.Events;

public class GameObject
{
    private readonly IEventEmitter _eventEmitter;

    internal GameObject(IEventEmitter eventEmitter, GameObjectId objectId)
    {
        _eventEmitter = eventEmitter;
        _objectId = objectId;
    }

    private GameObjectId _objectId;
    private string _name = string.Empty;
    private Vector3 _position;
    private float _rotation;
    private bool _isDead;
    private bool _isMounted;

    public GameObjectId ObjectId => _objectId;
    public string Name => _name;
    public Vector3 Position => _position;
    public float Rotation => _rotation;
    public bool IsDead => _isDead;
    public bool IsMounted => _isMounted;

    internal unsafe void Update(CSGameObject* gameObject)
    {
        Update(ref _name, new ReadOnlySeStringSpan(gameObject->GetName()).ExtractText(), GameObjectEvents.NameUpdated);
        Update(ref _position, gameObject->Position, GameObjectEvents.PositionUpdated);
        Update(ref _rotation, gameObject->Rotation, GameObjectEvents.RotationUpdated);
        Update(ref _isDead, gameObject->IsDead(), GameObjectEvents.IsDeadUpdated);
        Update(ref _isMounted, !gameObject->IsNotMounted(), GameObjectEvents.IsMountedUpdated);
    }

    private void Update<T>(ref T oldValue, T newValue, string eventName) where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            _eventEmitter.TriggerEvent(eventName, ValueChangedEventArgs<GameObject, T>.With(this, oldValue, newValue));
            oldValue = newValue;
        }
    }
}
