using System.Collections.Generic;
using System.IO;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfPcGender)] // n . . x
public class IfPcGenderPayload : HaselMacroPayload
{
    public BaseExpression? ObjectId { get; set; }
    public BaseExpression? CaseMale { get; set; }
    public BaseExpression? CaseFemale { get; set; }

    public override byte[] Encode()
    {
        if (ObjectId == null || CaseMale == null || CaseFemale == null)
            return [];

        return EncodeChunk(ObjectId, CaseMale, CaseFemale);
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        ObjectId = BaseExpression.Parse(reader.BaseStream);
        CaseMale = BaseExpression.Parse(reader.BaseStream);
        CaseFemale = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override unsafe HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (ObjectId == null)
            return new();

        var objectId = (uint)ObjectId.ResolveNumber(localParameterData);

        var chara = CharacterManager.Instance()->LookupBattleCharaByObjectId(objectId);
        if (chara == null)
            return CaseMale?.ResolveString(localParameterData) ?? new();

        var isMale = chara->Character.DrawData.CustomizeData.Sex == 0;
        return (isMale ? CaseMale : CaseFemale)?.ResolveString(localParameterData) ?? new();
    }
}
