using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class IAddonLifecycleExtensions
{
    extension(AddonRefreshArgs args)
    {
        public Span<AtkValue> GetAtkValues() => new((void*)args.AtkValues, (int)args.AtkValueCount);
    }

    extension(AddonSetupArgs args)
    {
        public Span<AtkValue> GetAtkValues() => new((void*)args.AtkValues, (int)args.AtkValueCount);
    }
}
