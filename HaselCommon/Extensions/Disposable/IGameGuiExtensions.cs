using Dalamud.Game.Gui;

namespace HaselCommon.Extensions;

public static partial class IGameGuiExtensions
{
    extension(IGameGui gameGui)
    {
        public IDisposable OnUpdate(Action<bool> handler)
        {
            void wrapper(object? _, bool action)
            {
                handler(action);
            }

            return EventExtensions.Subscribe(
                handler => gameGui.UiHideToggled += handler,
                handler => gameGui.UiHideToggled -= handler,
                (EventHandler<bool>)wrapper
            );
        }

        public IDisposable OnHoveredItemChanged(Action<ulong> handler)
        {
            void wrapper(object? _, ulong action)
            {
                handler(action);
            }

            return EventExtensions.Subscribe(
                handler => gameGui.HoveredItemChanged += handler,
                handler => gameGui.HoveredItemChanged -= handler,
                (EventHandler<ulong>)wrapper
            );
        }

        public IDisposable OnHoveredActionChanged(Action<HoveredAction> handler)
        {
            void wrapper(object? _, HoveredAction action)
            {
                handler(action);
            }

            return EventExtensions.Subscribe(
                handler => gameGui.HoveredActionChanged += handler,
                handler => gameGui.HoveredActionChanged -= handler,
                (EventHandler<HoveredAction>)wrapper
            );
        }

        public IDisposable OnAgentUpdate(Action<AgentUpdateFlag> handler)
        {
            return EventExtensions.Subscribe(
                handler => gameGui.AgentUpdate += handler,
                handler => gameGui.AgentUpdate -= handler,
                handler.Invoke
            );
        }
    }
}
