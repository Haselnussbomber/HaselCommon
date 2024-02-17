namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Float)] // n n s x
public class FloatPayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }
    public BaseExpression? Radix { get; set; }
    public BaseExpression? Separator { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Value == null || Radix == null || Separator == null)
            return new();

        var value = Value.ResolveNumber(localParameterData);
        var radix = Radix.ResolveNumber(localParameterData);
        var separator = Separator.ResolveString(localParameterData).ToString();

        var left = (int)(value / (float)radix);
        var right = value % radix;

        return $"{left}{separator}{right}";
    }
}
