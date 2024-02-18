namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Time)] // n x
public class TimePayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
        => this; // TODO: NYI
}
