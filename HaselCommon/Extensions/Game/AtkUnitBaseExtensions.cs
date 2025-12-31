using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static class AtkUnitBaseExtensions
{
    extension(AtkUnitBase addon)
    {
        public Vector2 Position => new(addon.X, addon.Y);
    }
}
