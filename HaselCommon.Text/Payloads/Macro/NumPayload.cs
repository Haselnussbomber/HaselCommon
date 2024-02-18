namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Num)] // n x
public class NumPayload : ParameterPayload
{
    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        var str = Parameter?.ResolveNumber(localParameters).ToString();
        return str == null
            ? (HaselSeString)this
            : (HaselSeString)str;
    }
}
