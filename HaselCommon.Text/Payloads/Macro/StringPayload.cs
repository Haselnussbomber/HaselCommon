namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.String)] // s x
public class StringPayload : ParameterPayload
{
    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        return Parameter?.ResolveString(localParameters) ?? new();
    }
}
