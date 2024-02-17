namespace HaselCommon.Text.Abstracts;

public abstract class ParameterPayload : HaselMacroPayload
{
    public BaseExpression? Parameter { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }
}
