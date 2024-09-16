using System.Collections.Generic;
using HaselCommon.Events;

namespace HaselCommon.Services.Events;

#pragma warning disable CS9113
internal class EventDispatcher(GameEvents GameEvents) : IDisposable
#pragma warning restore CS9113
{
    private HashSet<EventController> _controllers { get; } = [];

    public void Dispose()
    {
        _controllers.Clear();
        GC.SuppressFinalize(this);
    }

    internal void Register(EventController controller)
    {
        _controllers.Add(controller);
    }

    internal void Unregister(EventController controller)
    {
        _controllers.Remove(controller);
    }

    internal void DispatchEvent(string eventName, EventArgs? args = null)
    {
        foreach (var dispatcher in _controllers)
        {
            if (dispatcher.IsEventRegistered(eventName))
            {
                dispatcher.DispatchEvent(eventName, args);
            }
        }
    }
}
