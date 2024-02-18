namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Wait)] // n x
public class WaitPayload : HaselMacroPayload
{
    public ExpressionWrapper? Seconds { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }
}
