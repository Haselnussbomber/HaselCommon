namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.EdgeColorType)] // n x
public class EdgeColorTypePayload : HaselMacroPayload
{
    public BaseExpression? ColorType { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }
}
