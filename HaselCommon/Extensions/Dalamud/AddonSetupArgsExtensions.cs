using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AddonSetupArgsExtensions
{
    public static Span<AtkValue> GetAtkValues(this AddonSetupArgs args) => new((void*)args.AtkValues, (int)args.AtkValueCount);
}
