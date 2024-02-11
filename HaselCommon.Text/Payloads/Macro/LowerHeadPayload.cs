using System.Collections.Generic;
using System.IO;
using HaselCommon.Extensions;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.LowerHead)] // s x
public class LowerHeadPayload : HaselMacroPayload
{
    public BaseExpression? String { get; set; }

    public override byte[] Encode() => EncodeChunk(String);

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        String = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => String?.ResolveString(localParameterData).ToString()?.FirstCharToLower() ?? new HaselSeString();
}
