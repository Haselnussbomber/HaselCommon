namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Hyphen)] // -
public class HyphenPayload : CharacterPayload
{
    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
        => "-";
}
