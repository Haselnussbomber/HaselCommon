namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sec)] // n x
public class SecPayload : MacroPayload
{
    public Expression? Value { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Value == null)
            return new();

        return Value.ResolveNumber(localParameters).ToString("00"); // TODO: technically they cut it off if it's more than 2 digits
    }
}
