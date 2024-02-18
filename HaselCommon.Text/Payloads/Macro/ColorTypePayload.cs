namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.ColorType)] // n x
public class ColorTypePayload : HaselMacroPayload
{
    public ExpressionWrapper? ColorType { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }
}
