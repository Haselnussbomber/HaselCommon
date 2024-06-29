using Dalamud.Game;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.FrNoun)] // s . .
public class FrNounPayload : NounPayload
{
    public override ClientLanguage Language => ClientLanguage.French;
}
