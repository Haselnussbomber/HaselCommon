using System.Globalization;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Byte)] // n x
public class BytePayload : HaselMacroPayload
{
    public BaseExpression? Value { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Value == null)
            return new();

        var value = (float)(Value?.ResolveNumber(localParameterData) ?? 0);

        string[] suffix = ["", "K", "M", "G", "T"];
        int i;
        for (i = 0; i < suffix.Length && value >= 1024; i++, value /= 1024) ;

        var nfi = new NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalDigits = 1 };
        return $"{value.ToString("n", nfi)}{suffix[i]}";
    }
}
