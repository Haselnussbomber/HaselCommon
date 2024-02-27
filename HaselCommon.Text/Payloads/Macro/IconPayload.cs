namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Icon)] // n x
public class IconPayload : MacroPayload
{
    public Expression? IconId { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }
}
