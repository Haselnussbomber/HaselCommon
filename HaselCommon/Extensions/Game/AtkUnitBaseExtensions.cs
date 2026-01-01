using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AtkUnitBaseExtensions
{
    extension(AtkUnitBase unitBase)
    {
        public Vector2 Position => new(unitBase.X, unitBase.Y);

        public Vector2 Size
        {
            get
            {
                short width;
                short height;
                unitBase.GetSize(&width, &height, false);
                return new Vector2(width, height);
            }
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
        }
    }
}
