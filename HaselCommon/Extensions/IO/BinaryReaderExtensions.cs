using System.IO;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace HaselCommon.Extensions;

public static class BinaryReaderExtensions
{
    extension(BinaryReader reader)
    {
        public HalfVector2 ReadHalfVector2()
        {
            return new()
            {
                X = reader.ReadHalf(),
                Y = reader.ReadHalf()
            };
        }

        public HalfVector4 ReadHalfVector4()
        {
            return new()
            {
                X = reader.ReadHalf(),
                Y = reader.ReadHalf(),
                Z = reader.ReadHalf(),
                W = reader.ReadHalf()
            };
        }
    }
}
