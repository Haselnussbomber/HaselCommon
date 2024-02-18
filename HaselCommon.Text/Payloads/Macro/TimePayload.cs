namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Time)] // n x
public class TimePayload : HaselMacroPayload
{
    public ExpressionWrapper? Value { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
        => this; // TODO: NYI
}
