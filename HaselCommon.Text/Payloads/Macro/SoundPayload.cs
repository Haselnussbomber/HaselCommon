namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sound)] // n n
public class SoundPayload : HaselMacroPayload
{
    public BaseExpression? Arg1 { get; set; }
    public BaseExpression? SoundId { get; set; }
}
