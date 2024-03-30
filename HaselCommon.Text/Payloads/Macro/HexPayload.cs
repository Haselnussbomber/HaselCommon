namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Hex)] // n x
public class HexPayload : MacroPayload
{
    public Expression? Value { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Value == null)
            return new();

        var value = Value?.ResolveNumber(localParameters) ?? 0;
        return $"0x{value:X08}";
    }
}
