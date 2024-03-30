namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.ColorType)] // n x
public class ColorTypePayload : MacroPayload
{
    public Expression? ColorType { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }
}
