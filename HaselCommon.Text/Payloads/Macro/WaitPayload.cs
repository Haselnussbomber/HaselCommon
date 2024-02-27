namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Wait)] // n x
public class WaitPayload : MacroPayload
{
    public Expression? Seconds { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }
}
