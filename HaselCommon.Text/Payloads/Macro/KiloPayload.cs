using System.Globalization;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Kilo)] // . s x
public class KiloPayload : MacroPayload
{
    public Expression? Value { get; set; }
    public Expression? Separator { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Value == null || Separator == null)
            return new();

        var value = Value.ResolveNumber(localParameters);
        var separator = Separator.ResolveString(localParameters).ToString();

        var nfi = new NumberFormatInfo { NumberGroupSeparator = separator, NumberDecimalDigits = 0 };
        return $"{value.ToString("n", nfi)}";
    }
}
