namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.If)] // . . * x
public class IfPayload : MacroPayload
{
    public Expression? Condition { get; set; }
    public Expression? StatementTrue { get; set; }
    public Expression? StatementFalse { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Condition == null)
            return new();

        var isTruthy = Condition.ResolveNumber(localParameters) != 0;
        var statement = isTruthy ? StatementTrue : StatementFalse;

        return statement?.ResolveString(localParameters) ?? new();
    }
}
