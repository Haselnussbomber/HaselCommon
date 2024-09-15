using System.Collections.Generic;

namespace HaselCommon.Services.Events;

internal class EventController : EventEmitter, IEventController
{
    private readonly EventDispatcher _dispatcher;
    private readonly HashSet<string> _registeredEvents = [];
    private readonly Dictionary<string, HashSet<IEventController.EventHandler>> _handlers = [];
    private IEventController.EventHandler? _eventHandler;
    private bool _handleAllEvents;

    public EventController(EventDispatcher dispatcher) : base(dispatcher)
    {
        _dispatcher = dispatcher;
        _dispatcher.Register(this);
    }

    public void Dispose()
    {
        _dispatcher.Unregister(this);
        UnregisterAllEvents();
        _eventHandler = null;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public bool IsEventRegistered(string eventName)
    {
        return _handleAllEvents || _registeredEvents.Contains(eventName);
    }

    /// <inheritdoc />
    public void SetDefaultEventHandler(IEventController.EventHandler? handler)
    {
        _eventHandler = handler;
    }

    /// <inheritdoc />
    public void RegisterEvent(string eventName, IEventController.EventHandler? handler = null)
    {
        if (handler != null)
        {
            if (!_handlers.TryGetValue(eventName, out var handlers))
                _handlers.Add(eventName, handlers = []);

            handlers.Add(handler);
        }

        _registeredEvents.Add(eventName);
    }

    /// <inheritdoc />
    public void RegisterEvents(params string[] eventNames)
    {
        foreach (var eventName in eventNames)
        {
            _registeredEvents.Add(eventName);
        }
    }

    /// <inheritdoc />
    public void RegisterAllEvents()
    {
        _handleAllEvents = true;
    }

    /// <inheritdoc />
    public void Unsubscribe(string eventName)
    {
        _handlers.Remove(eventName);
        _registeredEvents.Remove(eventName);
    }

    /// <inheritdoc />
    public void Unsubscribe(string eventName, IEventController.EventHandler handler)
    {
        if (_handlers.TryGetValue(eventName, out var handlers))
        {
            handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                _handlers.Remove(eventName);
            }
        }
    }

    /// <inheritdoc />
    public void UnregisterAllEvents()
    {
        _handleAllEvents = false;
        _handlers.Clear();
        _registeredEvents.Clear();
    }

    internal void DispatchEvent(string name, EventArgs? args)
    {
        if (_handlers.TryGetValue(name, out var handlers))
        {
            foreach (var handler in handlers)
            {
                handler.Invoke(name, args);
            }
        }

        _eventHandler?.Invoke(name, args);
    }
}
