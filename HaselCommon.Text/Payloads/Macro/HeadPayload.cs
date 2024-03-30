namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Head)] // s x
public class HeadPayload : MacroPayload
{
    public Expression? String { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
        => String == null
            ? new()
            : (SeString)String.ResolveString(localParameters).ToString().FirstCharToUpper();
}
