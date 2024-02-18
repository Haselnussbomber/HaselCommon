namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.String)] // s x
public class StringPayload : ParameterPayload
{
    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        var str = Parameter?.ResolveString(localParameters).ToString();
        return str == null
            ? (HaselSeString)this
            : (HaselSeString)str;
    }
}
