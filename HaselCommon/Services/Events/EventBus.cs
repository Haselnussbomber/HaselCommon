using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using HaselCommon.Events;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public sealed partial class EventBus : IDisposable
{
    private const int SecondsUntilCleanup = 10;

    private static readonly string[] NoLogEvents = [
        AddonEvents.PreRequestedUpdate,
        AddonEvents.PostRequestedUpdate,
        AddonEvents.PreReceiveEvent,
        AddonEvents.PostReceiveEvent,
        AddonEvents.PreRefresh,
        AddonEvents.PostRefresh,
    ];

    private readonly ILogger<EventBus> _logger;
    private readonly IFramework _framework;

    private readonly List<WeakReference<EventDispatcher>> _eventDispatchers = [];
    private DateTime _nextCleanup = DateTime.UtcNow.AddSeconds(SecondsUntilCleanup);

    [AutoPostConstruct]
    private void Initialize()
    {
        _framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;
        _eventDispatchers.Clear();
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (_eventDispatchers.Count != 0 && _nextCleanup <= DateTime.UtcNow)
        {
            _eventDispatchers.RemoveAll(item => !item.TryGetTarget(out _));
            _nextCleanup = DateTime.UtcNow.AddSeconds(SecondsUntilCleanup);
        }
    }

    public void Register(EventDispatcher dispatcher)
    {
        _eventDispatchers.Add(new(dispatcher));
    }

    public void Unregister(EventDispatcher dispatcher)
    {
        _eventDispatchers.Remove(new(dispatcher));
    }

    public void Dispatch(string eventName)
    {
        if (!NoLogEvents.Contains(eventName))
            _logger.LogTrace("Dispatching {eventName}", eventName);

        foreach (var weakDispatcher in _eventDispatchers)
        {
            if (weakDispatcher.TryGetTarget(out var dispatcher))
                dispatcher.Dispatch(eventName);
        }
    }

    public void Dispatch<T>(string eventName, T arg)
    {
        if (!NoLogEvents.Contains(eventName))
            _logger.LogTrace("Dispatching {eventName} with args {arg}", eventName, arg);

        foreach (var weakDispatcher in _eventDispatchers)
        {
            if (weakDispatcher.TryGetTarget(out var dispatcher))
            {
                dispatcher.Dispatch(eventName);
                dispatcher.Dispatch(eventName, arg);
            }
        }
    }
}
