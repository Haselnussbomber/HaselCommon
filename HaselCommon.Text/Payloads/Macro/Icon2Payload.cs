namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Icon2)] // n N x
public class Icon2Payload : HaselMacroPayload
{
    public ExpressionWrapper? IconId { get; set; }
    public ExpressionWrapper? UnkNumber2 { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }
}
