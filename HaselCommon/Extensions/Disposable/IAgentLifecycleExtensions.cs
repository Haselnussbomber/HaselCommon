using Dalamud.Game.Agent.AgentArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using AgentEvent = Dalamud.Game.Agent.AgentEvent;
using DAgentId = Dalamud.Game.Agent.AgentId;
namespace HaselCommon.Extensions;

public static partial class IAgentLifecycleExtensions
{
    public delegate void AgentReceiveEventDelegate(AgentReceiveEventArgs args);
    public delegate void AgentShowDelegate(AgentArgs args);
    public delegate void AgentHideDelegate(AgentArgs args);
    public delegate void AgentUpdateDelegate(AgentArgs args);
    public delegate void AgentGameEventDelegate(AgentGameEventArgs args);
    public delegate void AgentLevelChangeDelegate(AgentLevelChangeArgs args);
    public delegate void AgentClassJobChangeDelegate(AgentClassJobChangeArgs args);

    extension(IAgentLifecycle agentLifecycle)
    {
        // ReceiveEvent (PreReceiveEvent, PostReceiveEvent)
        public IDisposable OnPreReceiveEvent(AgentReceiveEventDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentReceiveEventArgs>(AgentEvent.PreReceiveEvent, handler.Invoke, agentId);
        }

        public IDisposable OnPreReceiveEvent(AgentReceiveEventDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentReceiveEventArgs>(AgentEvent.PreReceiveEvent, handler.Invoke, agentIds);
        }

        public IDisposable OnPostReceiveEvent(AgentReceiveEventDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentReceiveEventArgs>(AgentEvent.PostReceiveEvent, handler.Invoke, agentId);
        }

        public IDisposable OnPostReceiveEvent(AgentReceiveEventDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentReceiveEventArgs>(AgentEvent.PostReceiveEvent, handler.Invoke, agentIds);
        }

        // ReceiveEventWithResult (PreReceiveEventWithResult, PostReceiveEventWithResult)
        public IDisposable OnPreReceiveEventWithResult(AgentReceiveEventDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentReceiveEventArgs>(AgentEvent.PreReceiveEventWithResult, handler.Invoke, agentId);
        }

        public IDisposable OnPreReceiveEventWithResult(AgentReceiveEventDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentReceiveEventArgs>(AgentEvent.PreReceiveEventWithResult, handler.Invoke, agentIds);
        }

        public IDisposable OnPostReceiveEventWithResult(AgentReceiveEventDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentReceiveEventArgs>(AgentEvent.PostReceiveEventWithResult, handler.Invoke, agentId);
        }

        public IDisposable OnPostReceiveEventWithResult(AgentReceiveEventDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentReceiveEventArgs>(AgentEvent.PostReceiveEventWithResult, handler.Invoke, agentIds);
        }

        // Show (PreShow, PostShow)
        public IDisposable OnPreShow(AgentShowDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PreShow, handler.Invoke, agentId);
        }

        public IDisposable OnPreShow(AgentShowDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PreShow, handler.Invoke, agentIds);
        }

        public IDisposable OnPostShow(AgentShowDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PostShow, handler.Invoke, agentId);
        }

        public IDisposable OnPostShow(AgentShowDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PostShow, handler.Invoke, agentIds);
        }

        // Hide (PreHide, PostHide)
        public IDisposable OnPreHide(AgentHideDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PreHide, handler.Invoke, agentId);
        }

        public IDisposable OnPreHide(AgentHideDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PreHide, handler.Invoke, agentIds);
        }

        public IDisposable OnPostHide(AgentHideDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PostHide, handler.Invoke, agentId);
        }

        public IDisposable OnPostHide(AgentHideDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PostHide, handler.Invoke, agentIds);
        }

        // Update (PreUpdate, PostUpdate)
        public IDisposable OnPreUpdate(AgentUpdateDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PreUpdate, handler.Invoke, agentId);
        }

        public IDisposable OnPreUpdate(AgentUpdateDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PreUpdate, handler.Invoke, agentIds);
        }

        public IDisposable OnPostUpdate(AgentUpdateDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PostUpdate, handler.Invoke, agentId);
        }

        public IDisposable OnPostUpdate(AgentUpdateDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentArgs>(AgentEvent.PostUpdate, handler.Invoke, agentIds);
        }

        // GameEvent (PreGameEvent, PostGameEvent)
        public IDisposable OnPreGameEvent(AgentGameEventDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentGameEventArgs>(AgentEvent.PreGameEvent, handler.Invoke, agentId);
        }

        public IDisposable OnPreGameEvent(AgentGameEventDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentGameEventArgs>(AgentEvent.PreGameEvent, handler.Invoke, agentIds);
        }

        public IDisposable OnPostGameEvent(AgentGameEventDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentGameEventArgs>(AgentEvent.PostGameEvent, handler.Invoke, agentId);
        }

        public IDisposable OnPostGameEvent(AgentGameEventDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentGameEventArgs>(AgentEvent.PostGameEvent, handler.Invoke, agentIds);
        }

        // LevelChange (PreLevelChange, PostLevelChange)
        public IDisposable OnPreLevelChange(AgentLevelChangeDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentLevelChangeArgs>(AgentEvent.PreLevelChange, handler.Invoke, agentId);
        }

        public IDisposable OnPreLevelChange(AgentLevelChangeDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentLevelChangeArgs>(AgentEvent.PreLevelChange, handler.Invoke, agentIds);
        }

        public IDisposable OnPostLevelChange(AgentLevelChangeDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentLevelChangeArgs>(AgentEvent.PostLevelChange, handler.Invoke, agentId);
        }

        public IDisposable OnPostLevelChange(AgentLevelChangeDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentLevelChangeArgs>(AgentEvent.PostLevelChange, handler.Invoke, agentIds);
        }

        // ClassJobChange (PreClassJobChange, PostClassJobChange)
        public IDisposable OnPreClassJobChange(AgentClassJobChangeDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentClassJobChangeArgs>(AgentEvent.PreClassJobChange, handler.Invoke, agentId);
        }

        public IDisposable OnPreClassJobChange(AgentClassJobChangeDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentClassJobChangeArgs>(AgentEvent.PreClassJobChange, handler.Invoke, agentIds);
        }

        public IDisposable OnPostClassJobChange(AgentClassJobChangeDelegate handler, AgentId agentId)
        {
            return agentLifecycle.RegisterAgentListener<AgentClassJobChangeArgs>(AgentEvent.PostClassJobChange, handler.Invoke, agentId);
        }

        public IDisposable OnPostClassJobChange(AgentClassJobChangeDelegate handler, params IEnumerable<AgentId> agentIds)
        {
            return agentLifecycle.RegisterAgentListener<AgentClassJobChangeArgs>(AgentEvent.PostClassJobChange, handler.Invoke, agentIds);
        }

        private IDisposable RegisterAgentListener<TArgs>(AgentEvent eventType, Action<TArgs> handler, AgentId agentId) where TArgs : AgentArgs
        {
            void wrapper(AgentEvent _, AgentArgs args)
            {
                handler((TArgs)args);
            }

            return EventExtensions.Subscribe(
                handler => agentLifecycle.RegisterListener(eventType, (DAgentId)agentId, handler),
                handler => agentLifecycle.UnregisterListener(eventType, (DAgentId)agentId, handler),
                (IAgentLifecycle.AgentEventDelegate)wrapper
            );
        }

        private IDisposable RegisterAgentListener<TArgs>(AgentEvent eventType, Action<TArgs> handler, IEnumerable<AgentId> agentIds) where TArgs : AgentArgs
        {
            void wrapper(AgentEvent _, AgentArgs args)
            {
                handler((TArgs)args);
            }

            return EventExtensions.Subscribe(
                handler => agentLifecycle.RegisterListener(eventType, agentIds.Cast<DAgentId>(), handler),
                handler => agentLifecycle.UnregisterListener(eventType, agentIds.Cast<DAgentId>(), handler),
                (IAgentLifecycle.AgentEventDelegate)wrapper
            );
        }
    }
}
