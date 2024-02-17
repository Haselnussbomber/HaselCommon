namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.SoftHyphen)] // -
public class SoftHyphenPayload : CharacterPayload
{
    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => this; // technically an invisible format character \u00AD, but ImGui is special and makes it visible...
}
