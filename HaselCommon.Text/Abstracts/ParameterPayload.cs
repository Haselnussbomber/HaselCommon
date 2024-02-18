namespace HaselCommon.Text.Abstracts;

public abstract class ParameterPayload : HaselMacroPayload
{
    public ExpressionWrapper? Parameter { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }
}
