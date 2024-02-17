using System.Globalization;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Kilo)] // . s x
public class KiloPayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }
    public BaseExpression? Separator { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Value == null || Separator == null)
            return new();

        var value = Value.ResolveNumber(localParameterData);
        var separator = Separator.ResolveString(localParameterData).ToString();

        var nfi = new NumberFormatInfo { NumberGroupSeparator = separator, NumberDecimalDigits = 0 };
        return $"{value.ToString("n", nfi)}";
    }
}
