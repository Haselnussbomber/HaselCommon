namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Icon2)] // n N x
public class Icon2Payload : HaselMacroPayload
{
    public BaseExpression? IconId { get; set; }
    public BaseExpression? UnkNumber2 { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }
}
