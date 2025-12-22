using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AddonSetupArgsExtensions
{
    extension(AddonSetupArgs args)
    {
        public Span<AtkValue> GetAtkValues() => new((void*)args.AtkValues, (int)args.AtkValueCount);
    }
}
