namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Digit)] // n n x
public class DigitPayload : MacroPayload
{
    public Expression? Value { get; set; }
    public Expression? TargetLength { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Value == null)
            return new();

        var value = Value?.ResolveNumber(localParameters) ?? 0;
        var targetLength = TargetLength?.ResolveNumber(localParameters) ?? 1;
        return value.ToString($"{new string('0', targetLength)}");
    }
}
