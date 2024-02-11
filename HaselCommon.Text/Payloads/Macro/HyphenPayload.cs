using System.Collections.Generic;
using HaselCommon.Text.Abstracts;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Hyphen)] // -
public class HyphenPayload : CharacterPayload
{
    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => "-";
}
