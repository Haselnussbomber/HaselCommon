using System.Collections.Generic;
using System.IO;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfSelf)] // n . . x
public class IfSelfPayload : HaselMacroPayload
{
    public BaseExpression? ObjectId { get; set; }
    public BaseExpression? CaseTrue { get; set; }
    public BaseExpression? CaseFalse { get; set; }

    public override byte[] Encode()
    {
        if (ObjectId == null || CaseTrue == null || CaseFalse == null)
            return [];

        return EncodeChunk(ObjectId, CaseTrue, CaseFalse);
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        ObjectId = BaseExpression.Parse(reader.BaseStream);
        CaseTrue = BaseExpression.Parse(reader.BaseStream);
        CaseFalse = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override unsafe HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (ObjectId == null)
            return new();

        var isSelf = (uint)ObjectId.ResolveNumber(localParameterData) == PlayerState.Instance()->ObjectId;
        return (isSelf ? CaseTrue : CaseFalse)?.ResolveString(localParameterData) ?? new();
    }
}
