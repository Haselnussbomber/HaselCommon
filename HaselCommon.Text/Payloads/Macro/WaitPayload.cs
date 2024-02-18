namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Wait)] // n x
public class WaitPayload : HaselMacroPayload
{
    public BaseExpression? Seconds { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }
}
