using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace HaselCommon.Events;

#pragma warning disable CS8618

public static class GameObjectEvents
{
    public const string Created = "GameObject.Created";
    public const string Destroyed = "GameObject.Destroyed";
    public const string NameUpdated = "GameObject.NameUpdated";
    public const string PositionUpdated = "GameObject.PositionUpdated";
    public const string RotationUpdated = "GameObject.RotationUpdated";
    public const string IsDeadUpdated = "GameObject.IsDeadUpdated";
    public const string IsMountedUpdated = "GameObject.IsMountedUpdated";

    public class GameObjectCreatedEventArgs : EventArgs
    {
        private static readonly GameObjectCreatedEventArgs Instance = new();
        private GameObjectCreatedEventArgs() { }

        public GameObject GameObject { get; private set; }

        internal static GameObjectCreatedEventArgs With(GameObject gameObject)
        {
            Instance.GameObject = gameObject;
            return Instance;
        }
    }

    public class GameObjectDestroyedEventArgs : EventArgs
    {
        private static readonly GameObjectDestroyedEventArgs Instance = new();
        private GameObjectDestroyedEventArgs() { }

        public GameObjectId ObjectId { get; private set; }

        internal static GameObjectDestroyedEventArgs With(GameObjectId objectId)
        {
            Instance.ObjectId = objectId;
            return Instance;
        }
    }
}
