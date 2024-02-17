namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Head)] // s x
public class HeadPayload : HaselMacroPayload
{
    public BaseExpression? String { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => String == null
            ? new()
            : (HaselSeString)String.ResolveString(localParameterData).ToString().FirstCharToUpper();
}
