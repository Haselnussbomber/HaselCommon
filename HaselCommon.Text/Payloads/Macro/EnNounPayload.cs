using Dalamud.Game;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.EnNoun)] // s . .
public class EnNounPayload : NounPayload
{
    public override ClientLanguage Language => ClientLanguage.English;
}
