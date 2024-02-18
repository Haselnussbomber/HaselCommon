namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.EdgeColorType)] // n x
public class EdgeColorTypePayload : HaselMacroPayload
{
    public ExpressionWrapper? ColorType { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }
}
