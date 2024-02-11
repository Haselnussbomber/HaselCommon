using Dalamud;
using HaselCommon.Text.Abstracts;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.EnNoun)] // s . .
public class EnNounPayload : NounPayload
{
    public override ClientLanguage Language => ClientLanguage.English;
}
