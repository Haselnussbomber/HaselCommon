namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.NonBreakingSpace)] // <nbsp>
public class NonBreakingSpacePayload : CharacterPayload
{
    public override SeString Resolve(List<Expression>? localParameters = null)
        => "\u00A0";
}
