namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Num)] // n x
public class NumPayload : ParameterPayload
{
    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        var str = Parameter?.ResolveNumber(localParameterData).ToString();
        return str == null
            ? (HaselSeString)this
            : (HaselSeString)str;
    }
}
