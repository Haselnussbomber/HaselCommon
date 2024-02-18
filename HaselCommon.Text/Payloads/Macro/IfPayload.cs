namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.If)] // . . * x
public class IfPayload : HaselMacroPayload
{
    public ExpressionWrapper? Condition { get; set; }
    public ExpressionWrapper? StatementTrue { get; set; }
    public ExpressionWrapper? StatementFalse { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (Condition == null)
            return new();

        var isTruthy = Condition.ResolveNumber(localParameters) != 0; // TODO: what if it's a string? can it be a string?
        var statement = isTruthy ? StatementTrue : StatementFalse;
        return statement?.ResolveString(localParameters) ?? new();
    }
}
