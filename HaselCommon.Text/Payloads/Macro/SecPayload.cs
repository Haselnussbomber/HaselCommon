namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sec)] // n x
public class SecPayload : HaselMacroPayload
{
    public ExpressionWrapper? Value { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (Value == null)
            return new();

        return Value.ResolveNumber(localParameters).ToString("00"); // TODO: technically they cut it off if it's more than 2 digits
    }
}
