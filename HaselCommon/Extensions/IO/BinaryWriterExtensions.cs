using System.IO;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace HaselCommon.Extensions;

public static class BinaryWriterExtensions
{
    extension(BinaryWriter writer)
    {
        public void Write(HalfVector2 vec)
        {
            writer.Write(vec.X);
            writer.Write(vec.Y);
        }

        public void Write(HalfVector4 vec)
        {
            writer.Write(vec.X);
            writer.Write(vec.Y);
            writer.Write(vec.Z);
            writer.Write(vec.W);
        }
    }
}
