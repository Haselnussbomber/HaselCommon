namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Float)] // n n s x
public class FloatPayload : HaselMacroPayload
{
    public ExpressionWrapper? Value { get; set; }
    public ExpressionWrapper? Radix { get; set; }
    public ExpressionWrapper? Separator { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (Value == null || Radix == null || Separator == null)
            return new();

        var value = Value.ResolveNumber(localParameters);
        var radix = Radix.ResolveNumber(localParameters);
        var separator = Separator.ResolveString(localParameters).ToString();

        var left = (int)(value / (float)radix);
        var right = value % radix;

        return $"{left}{separator}{right}";
    }
}
