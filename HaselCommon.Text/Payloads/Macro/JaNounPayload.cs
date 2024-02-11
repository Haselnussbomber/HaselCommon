using Dalamud;
using HaselCommon.Text.Abstracts;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.JaNoun)] // s . .
public class JaNounPayload : NounPayload
{
    public override ClientLanguage Language => ClientLanguage.Japanese;
}
