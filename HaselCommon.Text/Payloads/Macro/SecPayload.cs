namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sec)] // n x
public class SecPayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Value == null)
            return new();

        return Value.ResolveNumber(localParameterData).ToString("00"); // TODO: technically they cut it off if it's more than 2 digits
    }
}
