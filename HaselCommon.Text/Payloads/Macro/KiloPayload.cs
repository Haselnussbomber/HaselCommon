using System.Globalization;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Kilo)] // . s x
public class KiloPayload : HaselMacroPayload
{
    public ExpressionWrapper? Value { get; set; }
    public ExpressionWrapper? Separator { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (Value == null || Separator == null)
            return new();

        var value = Value.ResolveNumber(localParameters);
        var separator = Separator.ResolveString(localParameters).ToString();

        var nfi = new NumberFormatInfo { NumberGroupSeparator = separator, NumberDecimalDigits = 0 };
        return $"{value.ToString("n", nfi)}";
    }
}
