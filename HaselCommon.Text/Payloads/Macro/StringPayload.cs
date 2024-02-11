using System.Collections.Generic;
using HaselCommon.Text.Abstracts;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.String)] // s x
public class StringPayload : ParameterPayload
{
    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        var str = Parameter?.ResolveString(localParameterData).ToString();
        return str == null
            ? (HaselSeString)this
            : (HaselSeString)str;
    }
}
