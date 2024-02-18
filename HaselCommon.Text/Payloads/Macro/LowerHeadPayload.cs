namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.LowerHead)] // s x
public class LowerHeadPayload : HaselMacroPayload
{
    public ExpressionWrapper? String { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
        => String?.ResolveString(localParameters).ToString()?.FirstCharToLower() ?? new HaselSeString();
}
