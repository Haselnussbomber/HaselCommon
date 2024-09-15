using System.Collections.Generic;

namespace HaselCommon.Services.Events;

internal class EventDispatcher : IDisposable
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
