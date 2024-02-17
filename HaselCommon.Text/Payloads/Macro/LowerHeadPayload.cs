namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.LowerHead)] // s x
public class LowerHeadPayload : HaselMacroPayload
{
    public BaseExpression? String { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => String?.ResolveString(localParameterData).ToString()?.FirstCharToLower() ?? new HaselSeString();
}
