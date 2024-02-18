namespace HaselCommon.Text.Extensions;

public static class StringExpressionExtensions
{
    public static int ResolveNumber(this StringExpression expr, List<ExpressionWrapper>? localParameters = null)
        => int.Parse(expr.ResolveString(localParameters).ToString());

    public static HaselSeString ResolveString(this StringExpression expr, List<ExpressionWrapper>? localParameters = null)
        => HaselSeString.Parse(expr.Value.RawData).Resolve(localParameters);
}
