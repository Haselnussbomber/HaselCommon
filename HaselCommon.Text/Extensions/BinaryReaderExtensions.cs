using System.IO;
using HaselCommon.Text.Enums;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Extensions;

public static class BinaryReaderExtensions
{
    public static byte PeekByte(this BinaryReader reader)
    {
        var value = reader.ReadByte();
        reader.BaseStream.Position--;
        return value;
    }

    public static void GetPayloadInfo(this BinaryReader reader, out MacroCodes macroCode, out uint dataSizeLength, out uint dataSize)
    {
        var startPos = reader.BaseStream.Position;

        if (reader.ReadByte() != 0x02)
            throw new Exception("Expected START_BYTE");

        macroCode = (MacroCodes)reader.ReadByte();
        dataSizeLength = reader.PeekIntegerExpressionLength(); // dataSize itself
        dataSize = reader.ReadIntegerExpression();
        reader.BaseStream.Position += dataSize;

        if (reader.ReadByte() != 0x03)
            throw new Exception("Expected END_BYTE");

        reader.BaseStream.Position = startPos;
    }

    public static long GetRemaining(this BinaryReader reader)
        => reader.BaseStream.Length - reader.BaseStream.Position;

    public static bool IsEndOfChunk(this BinaryReader reader)
        => reader.GetRemaining() <= 1;

    public static uint PeekIntegerExpressionLength(this BinaryReader reader)
        => reader.PeekByte() switch
        {
            < 0xD0 => 1, // TODO: why is that < 0xC0 in "80 F9 80 73 06"?
            < 0xE0 => 2,
            < 0xF0 => 3,
            < 0xF8 => 4,
            < 0xFC => 5,
            < 0xFE => 6,
            _ => 1
        };

    public static uint ReadIntegerExpression(this BinaryReader reader)
        => IntegerExpression.Parse(reader.ReadByte(), reader.BaseStream).Value;

    public static void SeekToPayloadData(this BinaryReader reader)
    {
        reader.BaseStream.Position += 2; // START_BYTE + MacroCode
        reader.ReadIntegerExpression(); // DataSize
    }
}
