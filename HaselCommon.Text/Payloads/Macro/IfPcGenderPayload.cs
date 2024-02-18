using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfPcGender)] // n . . x
public class IfPcGenderPayload : HaselMacroPayload
{
    public BaseExpression? ObjectId { get; set; }
    public BaseExpression? CaseMale { get; set; }
    public BaseExpression? CaseFemale { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override unsafe HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (ObjectId == null)
            return new();

        var objectId = (uint)ObjectId.ResolveNumber(localParameters);

        var chara = CharacterManager.Instance()->LookupBattleCharaByObjectId(objectId);
        if (chara == null)
            return CaseMale?.ResolveString(localParameters) ?? new();

        var isMale = chara->Character.DrawData.CustomizeData.Sex == 0;
        return (isMale ? CaseMale : CaseFemale)?.ResolveString(localParameters) ?? new();
    }
}
