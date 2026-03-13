using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class IAddonLifecycleExtensions
{
    
    extension(AddonArgs args)
    {
        public T* GetAddon<T>() where T : unmanaged => (T*)args.Addon.Address;
    }

    extension(AddonRefreshArgs args)
    {
        public Span<AtkValue> GetAtkValues() => new((void*)args.AtkValues, (int)args.AtkValueCount);
    }

    extension(AddonSetupArgs args)
    {
        public Span<AtkValue> GetAtkValues() => new((void*)args.AtkValues, (int)args.AtkValueCount);
    }

    extension(AddonReceiveEventArgs args)
    {
        public AtkEventType EventType => (AtkEventType)args.AtkEventType;
        public T* GetEventData<T>() where T : unmanaged => (T*)args.AtkEventData;
    }
}
