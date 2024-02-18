namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.HeadAll)] // s x
public class HeadAllPayload : HaselMacroPayload
{
    public ExpressionWrapper? String { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (String == null)
            return new();

        var splitted = String.ResolveString(localParameters).ToString().Split(' ');

        for (var i = 0; i < splitted.Length; i++)
            splitted[i] = splitted[i].FirstCharToUpper();

        return string.Join(' ', splitted);
    }
}
