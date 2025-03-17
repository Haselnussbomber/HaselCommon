using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using HaselCommon.Services;

namespace HaselCommon.Events;

[RegisterSingleton<IEventProvider>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
internal sealed unsafe partial class AddonEventsProvider : IEventProvider
{
    private readonly EventDispatcher _eventDispatcher;
    private readonly IAddonLifecycle _addonLifecycle;
    private readonly AddonObserver _addonObserver;

    [AutoPostConstruct]
    private void Initialize()
    {
        _addonObserver.AddonOpen += OnAddonOpen;
        _addonObserver.AddonClose += OnAddonClose;

        _addonLifecycle.RegisterListener(AddonEvent.PreSetup, OnAddonEvent);
        _addonLifecycle.RegisterListener(AddonEvent.PostSetup, OnAddonEvent);
        // _addonLifecycle.RegisterListener(AddonEvent.PreUpdate, OnAddonEvent);
        // _addonLifecycle.RegisterListener(AddonEvent.PostUpdate, OnAddonEvent);
        // _addonLifecycle.RegisterListener(AddonEvent.PreDraw, OnAddonEvent);
        // _addonLifecycle.RegisterListener(AddonEvent.PostDraw, OnAddonEvent);
        _addonLifecycle.RegisterListener(AddonEvent.PreFinalize, OnAddonEvent);
        _addonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, OnAddonEvent);
        _addonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, OnAddonEvent);
        _addonLifecycle.RegisterListener(AddonEvent.PreRefresh, OnAddonEvent);
        _addonLifecycle.RegisterListener(AddonEvent.PostRefresh, OnAddonEvent);
        _addonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, OnAddonEvent);
        _addonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, OnAddonEvent);
    }

    public void Dispose()
    {
        _addonObserver.AddonOpen -= OnAddonOpen;
        _addonObserver.AddonClose -= OnAddonClose;

        _addonLifecycle.UnregisterListener(AddonEvent.PreSetup, OnAddonEvent);
        _addonLifecycle.UnregisterListener(AddonEvent.PostSetup, OnAddonEvent);
        // _addonLifecycle.UnregisterListener(AddonEvent.PreUpdate, OnAddonEvent);
        // _addonLifecycle.UnregisterListener(AddonEvent.PostUpdate, OnAddonEvent);
        // _addonLifecycle.UnregisterListener(AddonEvent.PreDraw, OnAddonEvent);
        // _addonLifecycle.UnregisterListener(AddonEvent.PostDraw, OnAddonEvent);
        _addonLifecycle.UnregisterListener(AddonEvent.PreFinalize, OnAddonEvent);
        _addonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, OnAddonEvent);
        _addonLifecycle.UnregisterListener(AddonEvent.PostRequestedUpdate, OnAddonEvent);
        _addonLifecycle.UnregisterListener(AddonEvent.PreRefresh, OnAddonEvent);
        _addonLifecycle.UnregisterListener(AddonEvent.PostRefresh, OnAddonEvent);
        _addonLifecycle.UnregisterListener(AddonEvent.PreReceiveEvent, OnAddonEvent);
        _addonLifecycle.UnregisterListener(AddonEvent.PostReceiveEvent, OnAddonEvent);
    }

    private void OnAddonOpen(string addonName)
    {
        _eventDispatcher.Trigger(AddonEvents.Open, new AddonEvents.AddonEventArgs
        {
            AddonName = addonName,
        });
    }

    private void OnAddonClose(string addonName)
    {
        _eventDispatcher.Trigger(AddonEvents.Close, new AddonEvents.AddonEventArgs
        {
            AddonName = addonName,
        });
    }

    private void OnAddonEvent(AddonEvent type, AddonArgs args)
    {
        switch (type)
        {
            case AddonEvent.PreSetup:
                _eventDispatcher.Trigger(AddonEvents.PreSetup, new AddonEvents.AddonEventArgs
                {
                    AddonName = args.AddonName,
                });
                break;

            case AddonEvent.PostSetup:
                _eventDispatcher.Trigger(AddonEvents.PostSetup, new AddonEvents.AddonEventArgs
                {
                    AddonName = args.AddonName,
                });
                break;

            case AddonEvent.PreFinalize:
                _eventDispatcher.Trigger(AddonEvents.PreFinalize, new AddonEvents.AddonEventArgs
                {
                    AddonName = args.AddonName,
                });
                break;

            case AddonEvent.PreRequestedUpdate:
                _eventDispatcher.Trigger(AddonEvents.PreRequestedUpdate, new AddonEvents.AddonEventArgs
                {
                    AddonName = args.AddonName,
                });
                break;

            case AddonEvent.PostRequestedUpdate:
                _eventDispatcher.Trigger(AddonEvents.PostRequestedUpdate, new AddonEvents.AddonEventArgs
                {
                    AddonName = args.AddonName,
                });
                break;

            case AddonEvent.PreRefresh when args is AddonRefreshArgs refreshArgs:
                _eventDispatcher.Trigger(AddonEvents.PreRefresh, new AddonEvents.RefreshArgs
                {
                    AddonName = refreshArgs.AddonName,
                    AtkValues = refreshArgs.AtkValues,
                    AtkValueCount = refreshArgs.AtkValueCount,
                });
                break;

            case AddonEvent.PostRefresh when args is AddonRefreshArgs refreshArgs:
                _eventDispatcher.Trigger(AddonEvents.PostRefresh, new AddonEvents.RefreshArgs
                {
                    AddonName = refreshArgs.AddonName,
                    AtkValues = refreshArgs.AtkValues,
                    AtkValueCount = refreshArgs.AtkValueCount,
                });
                break;

            case AddonEvent.PreReceiveEvent when args is AddonReceiveEventArgs receiveEventArgs:
                _eventDispatcher.Trigger(AddonEvents.PreReceiveEvent, new AddonEvents.EventArgs
                {
                    AddonName = receiveEventArgs.AddonName,
                    EventType = (AtkEventType)receiveEventArgs.AtkEventType,
                    Data = (Pointer<AtkEventData>)(AtkEventData*)receiveEventArgs.Data,
                });
                break;

            case AddonEvent.PostReceiveEvent when args is AddonReceiveEventArgs receiveEventArgs:
                _eventDispatcher.Trigger(AddonEvents.PostReceiveEvent, new AddonEvents.EventArgs
                {
                    AddonName = receiveEventArgs.AddonName,
                    EventType = (AtkEventType)receiveEventArgs.AtkEventType,
                    Data = (Pointer<AtkEventData>)(AtkEventData*)receiveEventArgs.Data,
                });
                break;
        }
    }
}
