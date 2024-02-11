using System.Collections.Generic;
using System.Globalization;
using System.IO;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Byte)] // n x
public class BytePayload : HaselMacroPayload
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

        var value = (float)(Value?.ResolveNumber(localParameterData) ?? 0);

        string[] suffix = ["", "K", "M", "G", "T"];
        int i;
        for (i = 0; i < suffix.Length && value >= 1024; i++, value /= 1024) ;

        var nfi = new NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalDigits = 1 };
        return $"{value.ToString("n", nfi)}{suffix[i]}";
    }
}
