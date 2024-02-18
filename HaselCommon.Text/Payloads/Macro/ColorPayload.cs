namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Color)] // n N x
public class ColorPayload : HaselMacroPayload
{
    public BaseExpression? Color { get; set; }
    public BaseExpression? Arg2 { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
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
                sb.Append(Color?.HaselToString());
            }
        }

        if (Arg2 != null)
        {
            sb.Append(',');
            sb.Append(Arg2.HaselToString());
        }
        
        sb.Append(')');
        sb.Append('>');

        return sb.ToString();
    }
}
