namespace HaselCommon.Text;

public class ExpressionWrapper(BaseExpression expression)
{
    public BaseExpression Expression { get; set; } = expression;

    public override string ToString()
        => Expression.ToString() ?? string.Empty;

    public static implicit operator ExpressionWrapper(Lumina.Text.SeString str) => new(new StringExpression(str));
    public static implicit operator ExpressionWrapper(string str) => new(new StringExpression(new(str)));
    public static implicit operator ExpressionWrapper(int value) => new(new IntegerExpression((uint)value));
    public static implicit operator ExpressionWrapper(uint value) => new(new IntegerExpression(value));
    //public static implicit operator ExpressionWrapper(HaselSeString str) => new(new StringExpression(new(str.Encode())));

    public static implicit operator BaseExpression(ExpressionWrapper wrapper) => wrapper.Expression;
}
