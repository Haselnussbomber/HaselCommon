namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Hyphen)] // -
public class HyphenPayload : CharacterPayload
{
    public override SeString Resolve(List<Expression>? localParameters = null)
        => "-";
}
