using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AtkUnitBaseExtensions
{
    extension(ref AtkUnitBase unitBase)
    {
        public Vector2 Position
        {
            get => new(unitBase.X, unitBase.Y);
            set => unitBase.SetPosition((short)value.X, (short)value.Y);
        }

        public Vector2 Size
        {
            get
            {
                short width;
                short height;
                unitBase.GetSize(&width, &height, false);
                return new Vector2(width, height);
            }

            set => unitBase.SetSize((ushort)value.X, (ushort)value.Y);
        }

        public Vector2 ScaledSize
        {
            get
            {
                short width;
                short height;
                unitBase.GetSize(&width, &height, true);
                return new Vector2(width, height);
            }

            set => unitBase.SetSize((ushort)(value.X * unitBase.Scale), (ushort)(value.Y * unitBase.Scale));
        }
    }
}
