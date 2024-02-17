namespace HaselCommon.Text.Extensions;

public static class BinaryReaderExtensions
{
    public static void GetPayloadInfo(this BinaryReader reader, out MacroCodes macroCode, out uint dataSizeLength, out uint dataSize)
    {
        var startPos = reader.BaseStream.Position;

        if (reader.ReadByte() != 0x02)
            throw new Exception("Expected START_BYTE");

        macroCode = (MacroCodes)reader.ReadByte();
        var posBeforeSize = reader.BaseStream.Position;
        dataSize = reader.ReadIntegerExpression();
        dataSizeLength = (uint)(reader.BaseStream.Position - posBeforeSize); // dataSize itself
        reader.BaseStream.Position += dataSize;

        if (reader.ReadByte() != 0x03)
            throw new Exception("Expected END_BYTE");

        reader.BaseStream.Position = startPos;
    }

    public static long GetRemaining(this BinaryReader reader)
        => reader.BaseStream.Length - reader.BaseStream.Position;

    public static bool IsEndOfChunk(this BinaryReader reader)
        => reader.GetRemaining() <= 1;

    public static uint ReadIntegerExpression(this BinaryReader reader)
        => IntegerExpression.Parse(reader.ReadByte(), reader.BaseStream).Value;
}
