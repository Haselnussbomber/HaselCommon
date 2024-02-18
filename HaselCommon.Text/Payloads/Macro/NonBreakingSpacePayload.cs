namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.NonBreakingSpace)] // <nbsp>
public class NonBreakingSpacePayload : CharacterPayload
{
    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
        => "\u00A0";
}
