using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfSelf)] // n . . x
public class IfSelfPayload : MacroPayload
{
    public Expression? EntityId { get; set; }
    public Expression? CaseTrue { get; set; }
    public Expression? CaseFalse { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override unsafe SeString Resolve(List<Expression>? localParameters = null)
    {
        if (EntityId == null)
            return new();

        var isSelf = (uint)EntityId.ResolveNumber(localParameters) == PlayerState.Instance()->EntityId;
        return (isSelf ? CaseTrue : CaseFalse)?.ResolveString(localParameters) ?? new();
    }
}
