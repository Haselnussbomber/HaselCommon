using System.Collections.Generic;
using System.IO;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfPcName)] // n . . . x
public class IfPcNamePayload : HaselMacroPayload
{
    public BaseExpression? ObjectId { get; set; }
    public BaseExpression? Name { get; set; }
    public BaseExpression? CaseTrue { get; set; }
    public BaseExpression? CaseFalse { get; set; }

    public override byte[] Encode()
    {
        if (ObjectId == null || Name == null || CaseTrue == null || CaseFalse == null)
            return [];

        return EncodeChunk(ObjectId, Name, CaseTrue, CaseFalse);
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        ObjectId = BaseExpression.Parse(reader.BaseStream);
        Name = BaseExpression.Parse(reader.BaseStream);
        CaseTrue = BaseExpression.Parse(reader.BaseStream);
        CaseFalse = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override unsafe HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (ObjectId == null || Name == null)
            return new();

        var objectId = (uint)ObjectId.ResolveNumber(localParameterData);

        var chara = CharacterManager.Instance()->LookupBattleCharaByObjectId(objectId);
        if (chara == null)
            return CaseFalse?.ResolveString(localParameterData) ?? new();

        var name = MemoryHelper.ReadStringNullTerminated((nint)chara->Character.GameObject.GetName());
        var isSameName = name == Name.ResolveString(localParameterData).ToString();
        return (isSameName ? CaseTrue : CaseFalse)?.ResolveString(localParameterData) ?? new();
    }
}
