using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfPcGender)] // n . . x
public class IfPcGenderPayload : MacroPayload
{
    public Expression? EntityId { get; set; }
    public Expression? CaseMale { get; set; }
    public Expression? CaseFemale { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override unsafe SeString Resolve(List<Expression>? localParameters = null)
    {
        if (EntityId == null)
            return new();

        var entityId = (uint)EntityId.ResolveNumber(localParameters);

        var chara = CharacterManager.Instance()->LookupBattleCharaByEntityId(entityId);
        if (chara == null)
            return CaseMale?.ResolveString(localParameters) ?? new();

        var isMale = chara->Character.DrawData.CustomizeData.Sex == 0;
        return (isMale ? CaseMale : CaseFemale)?.ResolveString(localParameters) ?? new();
    }
}
