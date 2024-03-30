namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Split)] // s s n x
public class SplitPayload : MacroPayload
{
    public Expression? Arg1 { get; set; }
    public Expression? Arg2 { get; set; }
    public Expression? Arg3 { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
        => this; // TODO: NYI
}
