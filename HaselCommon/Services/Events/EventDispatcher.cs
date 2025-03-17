using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

[RegisterTransient, AutoConstruct]
public sealed partial class EventDispatcher : IDisposable
{
    private readonly ILogger<EventDispatcher> _logger;
    private readonly EventBus _eventManager;

    private readonly Dictionary<string, List<Delegate>> _eventHandlers = [];

    [AutoPostConstruct]
    private void Initialize()
    {
        _eventManager.Register(this);
    }

    public void Dispose()
    {
        _eventManager.Unregister(this);
    }

    public void Subscribe(string eventName, Action handler)
    {
        if (!_eventHandlers.TryGetValue(eventName, out var handlers))
        {
            _eventHandlers.Add(eventName, handlers = []);
        }

        handlers.Add(handler);
    }

    public void Subscribe<T>(string eventName, Action<T> handler)
    {
        if (!_eventHandlers.TryGetValue(eventName, out var handlers))
        {
            _eventHandlers.Add(eventName, handlers = []);
        }

        handlers.Add(handler);
    }

    public void Unsubscribe(string eventName, Action handler)
    {
        if (!_eventHandlers.TryGetValue(eventName, out var handlers))
            return;

        if (handlers.Remove(handler) && handlers.Count == 0)
        {
            _eventHandlers.Remove(eventName);
        }
    }

    public void Unsubscribe<T>(string eventName, Action<T> handler)
    {
        if (!_eventHandlers.TryGetValue(eventName, out var handlers))
            return;

        if (handlers.Remove(handler) && handlers.Count == 0)
        {
            _eventHandlers.Remove(eventName);
        }
    }

    public void Unsubscribe(string eventName)
    {
        _eventHandlers.Remove(eventName);
    }

    public void UnsubscribeAll()
    {
        _eventHandlers.Clear();
    }

    public void Trigger(string eventName)
    {
        _eventManager.Dispatch(eventName);
    }

    public void Trigger<T>(string eventName, T arg)
    {
        _eventManager.Dispatch(eventName, arg);
    }

    public void Dispatch(string eventName)
    {
        if (!_eventHandlers.TryGetValue(eventName, out var handlers))
            return;

        foreach (var handler in handlers.OfType<Action>())
        {
            try
            {
                handler();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during raise of {handler}", handler.Method);
            }
        }
    }

    public void Dispatch<T>(string eventName, T arg)
    {
        if (!_eventHandlers.TryGetValue(eventName, out var handlers))
            return;

        foreach (var handler in handlers.OfType<Action<T>>())
        {
            try
            {
                handler.Invoke(arg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during raise of {handler}", handler.Method);
            }
        }
    }
}
