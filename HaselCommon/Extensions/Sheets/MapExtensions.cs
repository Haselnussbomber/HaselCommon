namespace HaselCommon.Extensions;

public static class MapExtensions
{
    extension(Map row)
    {
        // "41 0F BF C0 66 0F 6E D0 B8"
        private uint ConvertRawToMapPos(short offset, float value)
        {
            var scale = row.SizeFactor / 100.0f;
            return (uint)(10 - (int)(((value + offset) * scale + 1024f) * -0.2f / scale));
        }

        public uint ConvertRawToMapPosX(float x)
        {
            return row.ConvertRawToMapPos(row.OffsetX, x);
        }

        // tip: you probably want to pass the Z coord
        public uint ConvertRawToMapPosY(float y)
        {
            return row.ConvertRawToMapPos(row.OffsetY, y);
        }
    }
}
