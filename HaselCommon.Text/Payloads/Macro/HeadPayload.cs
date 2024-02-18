namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Head)] // s x
public class HeadPayload : HaselMacroPayload
{
    public ExpressionWrapper? String { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
        => String == null
            ? new()
            : (HaselSeString)String.ResolveString(localParameters).ToString().FirstCharToUpper();
}
