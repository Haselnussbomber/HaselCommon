using System.Collections.Generic;
using System.IO;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.If)] // . . * x
public class IfPayload : HaselMacroPayload
{
    public BaseExpression? Condition { get; set; }
    public BaseExpression? StatementTrue { get; set; }
    public BaseExpression? StatementFalse { get; set; }

    public override byte[] Encode()
    {
        if (Condition == null || StatementTrue == null || StatementFalse == null)
            return [];

        return EncodeChunk(Condition, StatementTrue, StatementFalse);
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        Condition = BaseExpression.Parse(reader.BaseStream);
        StatementTrue = BaseExpression.Parse(reader.BaseStream);
        StatementFalse = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Condition == null)
            return new();

        var isTruthy = Condition.ResolveNumber(localParameterData) != 0; // TODO: what if it's a string? can it be a string?
        var statement = isTruthy ? StatementTrue : StatementFalse;
        return statement?.ResolveString(localParameterData) ?? new();
    }
}
