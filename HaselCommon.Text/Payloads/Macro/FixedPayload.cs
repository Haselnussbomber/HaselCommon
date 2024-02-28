namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Fixed)] // n n . . .
public class FixedPayload : MacroPayload
{
    public Expression? Type { get; set; }
    public Expression? Arg2 { get; set; }
    public List<Expression> Args { get; set; } = [];

    public override byte[] Encode()
    {
        if (Type == null || Arg2 == null || Args.Count == 0)
            return [];

        using var stream = new MemoryStream();
        stream.WriteByte(START_BYTE);
        stream.WriteByte((byte)Code);

        using var chunkData = new MemoryStream();
        Type.Encode(chunkData);
        Arg2.Encode(chunkData);
        foreach (var c in Args)
            c.Encode(chunkData);
        chunkData.CopyStreamWithLengthTo(stream);

        stream.WriteByte(END_BYTE);

        return stream.ToArray();
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        Type = Expression.Parse(reader.BaseStream);
        Arg2 = Expression.Parse(reader.BaseStream);

        while (!reader.IsEndOfChunk())
            Args.Add(Expression.Parse(reader.BaseStream));

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }
}
