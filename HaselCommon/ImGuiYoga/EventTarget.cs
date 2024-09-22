using System.Linq;
using Dalamud.Plugin.Services;
using HaselCommon.ImGuiYoga.Events;

namespace HaselCommon.ImGuiYoga;

public class EventTarget : IDisposable
{
    public event Action<Event>? OnEvent;

    public virtual void Dispose()
    {
        RemoveAllEventListeners();
    }

    public virtual void DispatchEvent(Event evt)
    {
        if (OnEvent == null)
            return;

        foreach (var handler in OnEvent.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(evt);
            }
            catch (Exception ex)
            {
                Service.Get<IPluginLog>().Error(ex, "Unexpected error in DispatchEvent");
            }
        }
    }

    public void AddEventListener(Action<Event> callback)
    {
        OnEvent += callback;
    }

    public void RemoveEventListener(Action<Event> callback)
    {
        OnEvent -= callback;
    }

    public void RemoveAllEventListeners()
    {
        if (OnEvent == null)
            return;

        foreach (var eventHandler in OnEvent.GetInvocationList().Cast<Action<Event>>())
        {
            OnEvent -= eventHandler;
        }
    }
}
