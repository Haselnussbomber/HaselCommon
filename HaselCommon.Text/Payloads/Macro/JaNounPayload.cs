namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.JaNoun)] // s . .
public class JaNounPayload : NounPayload
{
    public override ClientLanguage Language => ClientLanguage.Japanese;
}
