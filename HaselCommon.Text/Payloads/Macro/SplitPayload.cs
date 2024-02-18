namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Split)] // s s n x
public class SplitPayload : HaselMacroPayload
{
    public ExpressionWrapper? Arg1 { get; set; }
    public ExpressionWrapper? Arg2 { get; set; }
    public ExpressionWrapper? Arg3 { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
        => this; // TODO: NYI
}
