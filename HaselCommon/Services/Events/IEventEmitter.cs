namespace HaselCommon.Services.Events;

public interface IEventEmitter
{
    /// <summary>
    /// Trigger an event.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="args">Optional event arguments.</param>
    void TriggerEvent(string eventName, EventArgs? args = null);
}
