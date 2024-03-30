namespace HaselCommon.Text.Payloads.Macro.Abstracts;

public abstract class ParameterPayload : MacroPayload
{
    public Expression? Parameter { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }
}
