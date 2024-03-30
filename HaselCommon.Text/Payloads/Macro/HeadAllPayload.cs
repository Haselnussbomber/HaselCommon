namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.HeadAll)] // s x
public class HeadAllPayload : MacroPayload
{
    public Expression? String { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (String == null)
            return new();

        var splitted = String.ResolveString(localParameters).ToString().Split(' ');

        for (var i = 0; i < splitted.Length; i++)
            splitted[i] = splitted[i].FirstCharToUpper();

        return string.Join(' ', splitted);
    }
}
