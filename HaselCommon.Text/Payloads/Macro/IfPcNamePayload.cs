using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfPcName)] // n . . . x
public class IfPcNamePayload : MacroPayload
{
    public Expression? ObjectId { get; set; }
    public Expression? Name { get; set; }
    public Expression? CaseTrue { get; set; }
    public Expression? CaseFalse { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override unsafe SeString Resolve(List<Expression>? localParameters = null)
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
