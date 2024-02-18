namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.If)] // . . * x
public class IfPayload : HaselMacroPayload
{
    public BaseExpression? Condition { get; set; }
    public BaseExpression? StatementTrue { get; set; }
    public BaseExpression? StatementFalse { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (Condition == null)
            return new();

        var isTruthy = Condition.ResolveNumber(localParameters) != 0; // TODO: what if it's a string? can it be a string?
        var statement = isTruthy ? StatementTrue : StatementFalse;
        return statement?.ResolveString(localParameters) ?? new();
    }
}
