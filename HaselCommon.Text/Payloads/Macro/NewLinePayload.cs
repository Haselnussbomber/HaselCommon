namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.NewLine)] // <br>
public class NewLinePayload : CharacterPayload
{
    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => "\n";
}
