using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static class AtkResNodeExtensions
{
    extension(AtkResNode node)
    {
        public Vector2 Position => new(node.X, node.Y);
        public Vector2 Size => new(node.GetWidth(), node.GetHeight());
    }
}
