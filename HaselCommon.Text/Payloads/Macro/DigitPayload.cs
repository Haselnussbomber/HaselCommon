namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Digit)] // n n x
public class DigitPayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }
    public BaseExpression? TargetLength { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Value == null)
            return new();

        var value = Value?.ResolveNumber(localParameterData) ?? 0;
        var targetLength = TargetLength?.ResolveNumber(localParameterData) ?? 1;
        return value.ToString($"{new string('0', targetLength)}");
    }
}
