using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AddonRefreshArgsExtensions
{
    public static Span<AtkValue> GetAtkValues(this AddonRefreshArgs args) => new((void*)args.AtkValues, (int)args.AtkValueCount);
}
