namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Lower)] // s x
public class LowerPayload : MacroPayload
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
            splitted[i] = splitted[i].FirstCharToLower();

        return string.Join(' ', splitted);
    }
}
