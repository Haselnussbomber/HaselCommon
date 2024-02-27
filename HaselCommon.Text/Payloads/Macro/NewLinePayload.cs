namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.NewLine)] // <br>
public class NewLinePayload : CharacterPayload
{
    public override SeString Resolve(List<Expression>? localParameters = null)
        => "\n";
}
