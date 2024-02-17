namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.ColorType)] // n x
public class ColorTypePayload : HaselMacroPayload
{
    public BaseExpression? ColorType { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }
}
