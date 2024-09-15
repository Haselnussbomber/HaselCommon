namespace HaselCommon.Services.Events;

internal class EventEmitter(EventDispatcher dispatcher) : IEventEmitter
{
    /// <inheritdoc />
    public void TriggerEvent(string eventName, EventArgs? args = null)
    {
        dispatcher.DispatchEvent(eventName, args);
    }
}
