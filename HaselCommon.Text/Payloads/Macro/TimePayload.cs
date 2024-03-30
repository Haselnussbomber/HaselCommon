namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Time)] // n x
public class TimePayload : MacroPayload
{
    public Expression? Value { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
        => this; // TODO: NYI
}
