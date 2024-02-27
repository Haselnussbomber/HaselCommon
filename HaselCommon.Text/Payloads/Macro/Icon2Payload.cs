namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Icon2)] // n N x
public class Icon2Payload : MacroPayload
{
    public Expression? IconId { get; set; }
    public Expression? UnkNumber2 { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }
}
