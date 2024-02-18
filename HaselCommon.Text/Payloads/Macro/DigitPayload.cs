namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Digit)] // n n x
public class DigitPayload : HaselMacroPayload
{
    public ExpressionWrapper? Value { get; set; }
    public ExpressionWrapper? TargetLength { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (Value == null)
            return new();

        var value = Value?.ResolveNumber(localParameters) ?? 0;
        var targetLength = TargetLength?.ResolveNumber(localParameters) ?? 1;
        return value.ToString($"{new string('0', targetLength)}");
    }
}
