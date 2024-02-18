namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Switch)] // . . .
public class SwitchPayload : HaselMacroPayload
{
    public BaseExpression? Condition { get; set; }
    public List<BaseExpression> Cases { get; set; } = [];

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

        Condition = BaseExpression.Parse(reader.BaseStream);

        while (!reader.IsEndOfChunk())
            Cases.Add(BaseExpression.Parse(reader.BaseStream));

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (Condition == null)
            return new();

        var caseIndex = Condition.ResolveNumber(localParameters) - 1;

        if (Cases.Count < caseIndex)
            return new();

        return Cases[caseIndex].ResolveString(localParameters);
    }
}
