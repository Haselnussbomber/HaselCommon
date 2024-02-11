using System.Collections.Generic;
using System.IO;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Ordinal)] // n x
public class OrdinalPayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }

    public override byte[] Encode() => EncodeChunk(Value);

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        Value = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Value == null)
            return new();

        //! https://stackoverflow.com/a/20175

        var value = Value?.ResolveNumber(localParameterData) ?? 0;

        if (value <= 0)
            return value.ToString();

        switch (value % 100)
        {
            case 11:
            case 12:
            case 13:
                return $"{value}th";
        }

        switch (value % 10)
        {
            case 1:
                return $"{value}st";
            case 2:
                return $"{value}nd";
            case 3:
                return $"{value}rd";
            default:
                return $"{value}th";
        }
    }
}
