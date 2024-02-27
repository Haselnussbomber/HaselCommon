namespace HaselCommon.Text;

// proxy class for BaseExpression to allow implicit casting

public class Expression(BaseExpression expression)
{
    public BaseExpression BaseExpression { get; set; } = expression;

    public ExpressionType ExpressionType => BaseExpression.ExpressionType;

    public void Encode(Stream stream)
        => BaseExpression.Encode(stream);

    public int ResolveNumber(List<Expression>? localParameters = null)
        => BaseExpression.ResolveNumber(localParameters);

    public SeString ResolveString(List<Expression>? localParameters = null)
        => BaseExpression.ResolveString(localParameters);

    public override string ToString()
        => BaseExpression.HaselToString() ?? string.Empty;

    public static implicit operator Expression(BaseExpression value) => new(value);
    public static implicit operator Expression(Lumina.Text.SeString str) => new(new StringExpression(str));
    public static implicit operator Expression(string str) => new(new StringExpression(new(str)));
    public static implicit operator Expression(int value) => new(new IntegerExpression((uint)value));
    public static implicit operator Expression(uint value) => new(new IntegerExpression(value));
    //public static implicit operator ExpressionWrapper(SeString str) => new(new StringExpression(new(str.Encode())));

    public static implicit operator BaseExpression(Expression wrapper) => wrapper.BaseExpression;
}
