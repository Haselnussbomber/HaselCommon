namespace HaselCommon.Text.Extensions;

public static class BinaryExpressionExtensions
{
    public static bool Resolve(this BinaryExpression expr, List<Expression>? localParameters = null)
    {
        if (expr.ExpressionType is ExpressionType.Equal or ExpressionType.NotEqual)
        {
            var op1 = expr.Operand1.ResolveString(localParameters);
            var op2 = expr.Operand2.ResolveString(localParameters);

            return expr.ExpressionType == ExpressionType.Equal
                ? op1 == op2
                : op1 != op2;
        }
        else
        {
            var op1 = expr.Operand1.ResolveNumber(localParameters);
            var op2 = expr.Operand2.ResolveNumber(localParameters);

            return expr.ExpressionType switch
            {
                ExpressionType.GreaterThanOrEqualTo => op1 >= op2,
                ExpressionType.GreaterThan => op1 > op2,
                ExpressionType.LessThanOrEqualTo => op1 <= op2,
                ExpressionType.LessThan => op1 < op2,
                _ => throw new NotImplementedException($"Unhandled BinaryExpression type 0x{(byte)expr.ExpressionType:X02}"),
            };
        }
    }
}
