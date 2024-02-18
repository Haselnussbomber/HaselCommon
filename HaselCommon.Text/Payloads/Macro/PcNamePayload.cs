using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.PcName)] // n x
public class PcNamePayload : HaselMacroPayload
{
    public ExpressionWrapper? ObjectId { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (ObjectId == null)
            return new();

        var objectId = (uint)ObjectId.ResolveNumber(localParameters);

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
