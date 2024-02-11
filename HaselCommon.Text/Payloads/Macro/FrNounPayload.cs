using Dalamud;
using HaselCommon.Text.Abstracts;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.FrNoun)] // s . .
public class FrNounPayload : NounPayload
{
    public override ClientLanguage Language => ClientLanguage.French;
}
