namespace HaselCommon.Services.Events;

public interface IEventController : IEventEmitter, IDisposable
{
    /// <summary>
    /// An event handler delegate which is called for an event.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="eventArgs">The optional event arguments.</param>
    delegate void EventHandler(string eventName, EventArgs? eventArgs);

    /// <summary>
    /// Checks whether an event is registered.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <returns><c>true</c> if the event is registered, <c>false</c> otherwise.</returns>
    bool IsEventRegistered(string eventName);

    /// <summary>
    /// Sets the default event handler called for all registred events of this EventRegistry.
    /// </summary>
    /// <param name="handler">The event handler.</param>
    void SetDefaultEventHandler(EventHandler? handler);

    /// <summary>
    /// Register an event.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="handler">The handler that is invoked when the event is dispatched.</param>
    void RegisterEvent(string eventName, EventHandler? handler = null);

    /// <summary>
    /// Register multiple events.
    /// </summary>
    /// <param name="eventNames">The event names.</param>
    void RegisterEvents(params string[] eventNames);

    /// <summary>
    /// Register for all events.
    /// Please know what you're doing.
    /// </summary>
    void RegisterAllEvents();

    /// <summary>
    /// Unregisters all event handlers for an event.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    void Unsubscribe(string eventName);

    /// <summary>
    /// Unregisters an event handler for an event.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="handler">The handler that should be unregistered.</param>
    void Unsubscribe(string eventName, EventHandler handler);

    /// <summary>
    /// Unregisters from all events.
    /// </summary>
    void UnregisterAllEvents();
}
