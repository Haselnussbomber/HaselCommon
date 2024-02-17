namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Icon)] // n x
public class IconPayload : HaselMacroPayload
{
    public BaseExpression? IconId { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }
}
