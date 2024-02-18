namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Split)] // s s n x
public class SplitPayload : HaselMacroPayload
{
    public BaseExpression? Arg1 { get; set; }
    public BaseExpression? Arg2 { get; set; }
    public BaseExpression? Arg3 { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
        => this; // TODO: NYI
}
