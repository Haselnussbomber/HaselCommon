namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Num)] // n x
public class NumPayload : ParameterPayload
{
    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        var str = Parameter?.ResolveNumber(localParameters).ToString();
        return str == null
            ? (SeString)this
            : (SeString)str;
    }
}
