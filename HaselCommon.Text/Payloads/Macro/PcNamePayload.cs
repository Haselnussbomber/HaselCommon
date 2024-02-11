using System.Collections.Generic;
using System.IO;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.PcName)] // n x
public class PcNamePayload : HaselMacroPayload
{
    public BaseExpression? ObjectId { get; set; }

    /// <inheritdoc/>
    public string Text
        => ObjectId is IntegerExpression integerExpression
            ? GetNameForObjectId(integerExpression.Value)
            : string.Empty;

    public override byte[] Encode() => EncodeChunk(ObjectId);

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        ObjectId = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (ObjectId == null)
            return new();

        var objectId = (uint)ObjectId.ResolveNumber(localParameterData);

        return GetNameForObjectId(objectId);
    }

    private static unsafe string GetNameForObjectId(uint objectId)
    {
        var chara = CharacterManager.Instance()->LookupBattleCharaByObjectId(objectId);
        return chara == null
            ? string.Empty
            : MemoryHelper.ReadStringNullTerminated((nint)chara->Character.GameObject.GetName());
    }
}
