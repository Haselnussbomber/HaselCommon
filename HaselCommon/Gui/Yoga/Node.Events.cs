using System.Collections.Generic;
using HaselCommon.Gui.Yoga.Events;

namespace HaselCommon.Gui.Yoga;

public partial class Node
{
    public delegate void EventHandler<T>(object? sender, T evt) where T : YogaEvent;

    private readonly Dictionary<Type, HashSet<Delegate>> _eventHandlers = [];
    private readonly List<YogaEvent> _bufferedEvents = [];

    public Node AddEventListener<T>(EventHandler<T> callback) where T : YogaEvent
    {
        if (!_eventHandlers.TryGetValue(typeof(T), out var actions))
            _eventHandlers.Add(typeof(T), actions = []);

        actions.Add(callback);

        return this;
    }

    public Node RemoveEventListener<T>() where T : YogaEvent
    {
        if (_eventHandlers.TryGetValue(typeof(T), out var actions))
        {
            actions.Clear();
            _eventHandlers.Remove(typeof(T));
        }

        return this;
    }

    public Node RemoveEventListener<T>(EventHandler<T> callback) where T : YogaEvent
    {
        if (_eventHandlers.TryGetValue(typeof(T), out var actions))
        {
            actions.Remove(callback);

            if (actions.Count == 0)
                _eventHandlers.Remove(typeof(T));
        }

        return this;
    }

    public void DispatchEvent<T>(T eventArgs) where T : YogaEvent
    {
        _bufferedEvents.Add(eventArgs);
    }

    private void ProcessEvents()
    {
        foreach (var evt in _bufferedEvents)
        {
            ProcessEvent(this, evt);
        }

        _bufferedEvents.Clear();
    }

    public virtual void ProcessEvent(object? sender, YogaEvent evt)
    {
        if (_eventHandlers.TryGetValue(evt.GetType(), out var actions))
        {
            foreach (var action in actions)
            {
                action.DynamicInvoke(sender, evt);
            }
        }

        if (evt.Bubbles)
        {
            Parent?.ProcessEvent(sender, evt);
        }
    }
}
