using System.Globalization;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Byte)] // n x
public class BytePayload : MacroPayload
{
    public Expression? Value { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Value == null)
            return new();

        var value = (float)(Value?.ResolveNumber(localParameters) ?? 0);

        string[] suffix = ["", "K", "M", "G", "T"];
        int i;
        for (i = 0; i < suffix.Length && value >= 1024; i++, value /= 1024) ;

        var nfi = new NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalDigits = 1 };
        return $"{value.ToString("n", nfi)}{suffix[i]}";
    }
}
