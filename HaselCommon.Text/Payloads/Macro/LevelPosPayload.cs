namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.LevelPos)] // n x
public class LevelPosPayload : HaselMacroPayload
{
    public BaseExpression? Arg1 { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => this; // TODO: NYI
}
