namespace HaselCommon.Text.Extensions;

public static class StringExpressionExtensions
{
    public static int ResolveNumber(this StringExpression expr, List<HaselSeString>? localParameterData = null)
        => int.Parse(expr.ResolveString(localParameterData).ToString());

    public static HaselSeString ResolveString(this StringExpression expr, List<HaselSeString>? localParameterData = null)
        => HaselSeString.Parse(expr.Value.RawData).Resolve(localParameterData);
}
