using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfSelf)] // n . . x
public class IfSelfPayload : HaselMacroPayload
{
    public ExpressionWrapper? ObjectId { get; set; }
    public ExpressionWrapper? CaseTrue { get; set; }
    public ExpressionWrapper? CaseFalse { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override unsafe HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (ObjectId == null)
            return new();

        var isSelf = (uint)ObjectId.ResolveNumber(localParameters) == PlayerState.Instance()->ObjectId;
        return (isSelf ? CaseTrue : CaseFalse)?.ResolveString(localParameters) ?? new();
    }
}
