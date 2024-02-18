
namespace HaselCommon.Text;

public class ExpressionWrapper(BaseExpression expression)
{
    public BaseExpression BaseExpression { get; set; } = expression;

    public ExpressionType ExpressionType => BaseExpression.ExpressionType;

    public void Encode(Stream stream)
        => BaseExpression.Encode(stream);

    public int ResolveNumber(List<ExpressionWrapper>? localParameters = null)
        => BaseExpression.ResolveNumber(localParameters);

    public HaselSeString ResolveString(List<ExpressionWrapper>? localParameters = null)
        => BaseExpression.ResolveString(localParameters);

    public override string ToString()
        => BaseExpression.HaselToString() ?? string.Empty;

    public static implicit operator ExpressionWrapper(BaseExpression value) => new(value);
    public static implicit operator ExpressionWrapper(Lumina.Text.SeString str) => new(new StringExpression(str));
    public static implicit operator ExpressionWrapper(string str) => new(new StringExpression(new(str)));
    public static implicit operator ExpressionWrapper(int value) => new(new IntegerExpression((uint)value));
    public static implicit operator ExpressionWrapper(uint value) => new(new IntegerExpression(value));
    //public static implicit operator ExpressionWrapper(HaselSeString str) => new(new StringExpression(new(str.Encode())));

    public static implicit operator BaseExpression(ExpressionWrapper wrapper) => wrapper.BaseExpression;
}
