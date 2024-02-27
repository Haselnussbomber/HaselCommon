namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Float)] // n n s x
public class FloatPayload : MacroPayload
{
    public Expression? Value { get; set; }
    public Expression? Radix { get; set; }
    public Expression? Separator { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
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
