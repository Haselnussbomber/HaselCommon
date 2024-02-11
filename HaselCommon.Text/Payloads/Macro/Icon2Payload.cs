using System.Collections.Generic;
using System.IO;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

// TODO: find out what the optional number parameter is
[SeStringPayload(MacroCodes.Icon2)] // n N x
public class Icon2Payload : HaselMacroPayload
{
    public BaseExpression? IconId { get; set; }
    public BaseExpression? UnkNumber2 { get; set; }

    public override byte[] Encode() => EncodeChunk(IconId, UnkNumber2);

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        IconId = BaseExpression.Parse(reader.BaseStream);

        if (!reader.IsEndOfChunk())
            UnkNumber2 = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => this;
}
