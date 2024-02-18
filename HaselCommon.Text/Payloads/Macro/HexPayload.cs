namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Hex)] // n x
public class HexPayload : HaselMacroPayload
{
    public ExpressionWrapper? Value { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (Value == null)
            return new();

        var value = Value?.ResolveNumber(localParameters) ?? 0;
        return $"0x{value:X08}";
    }
}
