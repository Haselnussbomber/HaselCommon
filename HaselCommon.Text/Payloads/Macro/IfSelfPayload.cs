using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfSelf)] // n . . x
public class IfSelfPayload : MacroPayload
{
    public Expression? ObjectId { get; set; }
    public Expression? CaseTrue { get; set; }
    public Expression? CaseFalse { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override unsafe SeString Resolve(List<Expression>? localParameters = null)
    {
        if (ObjectId == null)
            return new();

        var isSelf = (uint)ObjectId.ResolveNumber(localParameters) == PlayerState.Instance()->ObjectId;
        return (isSelf ? CaseTrue : CaseFalse)?.ResolveString(localParameters) ?? new();
    }
}
