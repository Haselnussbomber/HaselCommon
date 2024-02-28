namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Switch)] // . . .
public class SwitchPayload : MacroPayload
{
    public Expression? Condition { get; set; }
    public List<Expression> Cases { get; set; } = [];

    public override byte[] Encode()
    {
        if (Condition == null || Cases.Count == 0)
            return [];

        using var stream = new MemoryStream();
        stream.WriteByte(START_BYTE);
        stream.WriteByte((byte)Code);

        using var chunkData = new MemoryStream();
        Condition.Encode(chunkData);
        foreach (var c in Cases)
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

        Condition = Expression.Parse(reader.BaseStream);

        while (!reader.IsEndOfChunk())
            Cases.Add(Expression.Parse(reader.BaseStream));

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Condition == null)
            return new();

        var caseIndex = Condition.ResolveNumber(localParameters);

        if (caseIndex < 0 || Cases.Count < caseIndex)
            return new();

        return Cases[caseIndex].ResolveString(localParameters);
    }
}
