namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.DeNoun)] // s . .
public class DeNounPayload : NounPayload
{
    public override ClientLanguage Language => ClientLanguage.German;
}
