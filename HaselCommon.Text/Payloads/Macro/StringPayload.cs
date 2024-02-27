namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.String)] // s x
public class StringPayload : ParameterPayload
{
    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        var str = Parameter?.ResolveString(localParameters).ToString();
        return str == null
            ? (SeString)this
            : (SeString)str;
    }
}
