using System.Collections.Generic;
using System.IO;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Float)] // n n s x
public class FloatPayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }
    public BaseExpression? Radix { get; set; }
    public BaseExpression? Separator { get; set; }

    public override byte[] Encode()
    {
        if (Value == null || Radix == null || Separator == null)
            return [];

        return EncodeChunk(Value, Radix, Separator);
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        Value = BaseExpression.Parse(reader.BaseStream);
        Radix = BaseExpression.Parse(reader.BaseStream);
        Separator = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Value == null || Radix == null || Separator == null)
            return new();

        var value = Value.ResolveNumber(localParameterData);
        var radix = Radix.ResolveNumber(localParameterData);
        var separator = Separator.ResolveString(localParameterData).ToString();

        var left = (int)(value / (float)radix);
        var right = value % radix;

        return $"{left}{separator}{right}";
    }
}
