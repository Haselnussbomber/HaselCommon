namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Hex)] // n x
public class HexPayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Value == null)
            return new();

        var value = Value?.ResolveNumber(localParameterData) ?? 0;
        return $"0x{value:X08}";
    }
}
