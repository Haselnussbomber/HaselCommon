using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfPcName)] // n . . . x
public class IfPcNamePayload : HaselMacroPayload
{
    public BaseExpression? ObjectId { get; set; }
    public BaseExpression? Name { get; set; }
    public BaseExpression? CaseTrue { get; set; }
    public BaseExpression? CaseFalse { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override unsafe HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (ObjectId == null || Name == null)
            return new();

        var objectId = (uint)ObjectId.ResolveNumber(localParameters);

        var chara = CharacterManager.Instance()->LookupBattleCharaByObjectId(objectId);
        if (chara == null)
            return CaseFalse?.ResolveString(localParameters) ?? new();

        var name = MemoryHelper.ReadStringNullTerminated((nint)chara->Character.GameObject.GetName());
        var isSameName = name == Name.ResolveString(localParameters).ToString();
        return (isSameName ? CaseTrue : CaseFalse)?.ResolveString(localParameters) ?? new();
    }
}
