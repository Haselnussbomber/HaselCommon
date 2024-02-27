namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.EdgeColorType)] // n x
public class EdgeColorTypePayload : MacroPayload
{
    public Expression? ColorType { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }
}
