namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.HeadAll)] // s x
public class HeadAllPayload : HaselMacroPayload
{
    public BaseExpression? String { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (String == null)
            return new();

        var splitted = String.ResolveString(localParameterData).ToString().Split(' ');

        for (var i = 0; i < splitted.Length; i++)
            splitted[i] = splitted[i].FirstCharToUpper();

        return string.Join(' ', splitted);
    }
}
