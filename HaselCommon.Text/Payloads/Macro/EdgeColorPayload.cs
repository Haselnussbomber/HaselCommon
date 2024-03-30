namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.EdgeColor)] // n N x
public class EdgeColorPayload : MacroPayload
{
    public Expression? Color { get; set; }
    public Expression? Arg2 { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Color == null)
            return new();

        // stackcolor is used to pop the color from the stack, think of ImGui.PushStyleColor/PopStyleColor
        // as it can't be resolved, we just copy the expression if present
        // the game does this too in MacroDecoder.vf9
        var isStackcolor = Color.ExpressionType == ExpressionType.StackColor;
        var payload = new ColorPayload
        {
            Color = isStackcolor
                ? Color
                : new IntegerExpression((uint)Color.ResolveNumber(localParameters))
        };

        return payload;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append('<');
        sb.Append(Code.ToString().ToLower());
        sb.Append('(');

        if (Color != null)
        {
            if (Color is IntegerExpression integerExpression && integerExpression.Value == 0)
            {
                sb.Append("stackcolor");
            }
            else
            {
                sb.Append(Color.ToString());
            }
        }

        sb.Append(')');
        sb.Append('>');

        return sb.ToString();
    }
}
