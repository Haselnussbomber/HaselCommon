namespace HaselCommon.Text.Extensions;

public static class StringExpressionExtensions
{
    public static int ResolveNumber(this StringExpression expr, List<Expression>? localParameters = null)
        => int.Parse(expr.ResolveString(localParameters).ToString());

    public static SeString ResolveString(this StringExpression expr, List<Expression>? localParameters = null)
        => SeString.Parse(expr.Value.RawData).Resolve(localParameters);
}
