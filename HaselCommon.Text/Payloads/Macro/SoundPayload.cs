namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sound)] // n n
public class SoundPayload : HaselMacroPayload
{
    public ExpressionWrapper? IsJingle { get; set; }
    public ExpressionWrapper? SoundId { get; set; }
}
