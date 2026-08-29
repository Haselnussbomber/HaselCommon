using Dalamud.Game.Config;

namespace HaselCommon.Extensions;

public static partial class IGameConfigExtensions
{
    public delegate void GameConfigChangeDelegate(ConfigChangeEvent change);

    extension(IGameConfig gameConfig)
    {
        public IDisposable OnGameConfigChange(GameConfigChangeDelegate handler)
        {
            void wrapper(object? _, ConfigChangeEvent evt)
            {
                handler(evt);
            }

            return EventExtensions.Subscribe(
                handler => gameConfig.Changed += handler,
                handler => gameConfig.Changed -= handler,
                (EventHandler<ConfigChangeEvent>)wrapper
            );
        }
    }
}
