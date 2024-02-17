namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.NonBreakingSpace)] // <nbsp>
public class NonBreakingSpacePayload : CharacterPayload
{
    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => "\u00A0";
}
