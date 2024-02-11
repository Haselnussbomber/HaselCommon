using System.Collections.Generic;
using System.IO;
using HaselCommon.Structs;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.SetTime)] // n x
public class SetTimePayload : HaselMacroPayload
{
    public BaseExpression? Time { get; set; }

    public override byte[] Encode() => EncodeChunk(Time);

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        Time = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override unsafe HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Time != null)
        {
            var mt = MacroTime.Instance();
            mt->SetTime(DateTimeOffset.FromUnixTimeSeconds(Time.ResolveNumber(localParameterData)).DateTime);
        }

        return this;
    }
}
