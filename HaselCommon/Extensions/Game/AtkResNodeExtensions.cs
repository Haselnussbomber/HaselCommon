using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static class AtkResNodeExtensions
{
    extension(ref AtkResNode node)
    {
        public Vector2 Position
        {
            get => new(node.X, node.Y);
            set => node.SetPositionFloat(value.X, value.Y);
        }

        public Vector2 Size
        {
            get => new(node.GetWidth(), node.GetHeight());
            set
            {
                node.SetWidth((ushort)value.X);
                node.SetHeight((ushort)value.Y);
            }
        }
    }
}
