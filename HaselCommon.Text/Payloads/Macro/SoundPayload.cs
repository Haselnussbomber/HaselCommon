namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sound)] // n n
public class SoundPayload : MacroPayload
{
    public Expression? IsJingle { get; set; }
    public Expression? SoundId { get; set; }
}
