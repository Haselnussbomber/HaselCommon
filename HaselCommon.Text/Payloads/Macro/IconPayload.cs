namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Icon)] // n x
public class IconPayload : HaselMacroPayload
{
    public ExpressionWrapper? IconId { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }
}
