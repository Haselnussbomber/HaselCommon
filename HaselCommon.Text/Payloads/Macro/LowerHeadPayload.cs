namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.LowerHead)] // s x
public class LowerHeadPayload : MacroPayload
{
    public Expression? String { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
        => String?.ResolveString(localParameters).ToString()?.FirstCharToLower() ?? new SeString();
}
