using FFXIVClientStructs.FFXIV.Component.Text;

namespace HaselCommon.Text.Extensions;

// ReadExpression: "E8 ?? ?? ?? ?? 0F B7 55 18"
// ReadParameter: "E8 ?? ?? ?? ?? 49 8B 45 68"
// ReadPackedInteger (0xF0-0xFE): "0F B6 01 4C 8D 49 01 05"

// ExpressionType 0xEE is not an expression, it's used for icons. basically it's the UTF-8 Private Use Area, but 0xE0 was already used for GreaterThanOrEqualTo

// TODO: find a better way to Resolve() expressions...

public static unsafe class BaseExpressionExtensions
{
    public static string HaselToString(this BaseExpression expr)
    {
        return expr is StringExpression stringExpression
            ? HaselSeString.Parse(stringExpression.Value.RawData).ToString()
            : expr.ToString()!;
    }

    public static int ResolveNumber(this BaseExpression expr, List<ExpressionWrapper>? localParameters = null)
    {
        if (expr is IntegerExpression integerExpression)
            return (int)integerExpression.Value;

        if (expr is StringExpression stringExpression)
            return stringExpression.ResolveNumber(localParameters);

        if (expr is BinaryExpression binaryExpression)
            return binaryExpression.Resolve(localParameters) ? 1 : 0;

        if (expr is ParameterExpression parameterExpression)
            return parameterExpression.ResolveNumber(localParameters);

        return expr.ExpressionType switch
        {
            ExpressionType.Millisecond => DateTime.Now.Millisecond,
            ExpressionType.Second => MacroDecoder.GetMacroTime()->tm_sec,
            ExpressionType.Minute => MacroDecoder.GetMacroTime()->tm_min,
            ExpressionType.Hour => MacroDecoder.GetMacroTime()->tm_hour,
            ExpressionType.Day => ((FixedTm*)MacroDecoder.GetMacroTime())->tm_mday,
            ExpressionType.Weekday => ((FixedTm*)MacroDecoder.GetMacroTime())->tm_wday,
            ExpressionType.Month => MacroDecoder.GetMacroTime()->tm_mon + 1,
            ExpressionType.Year => MacroDecoder.GetMacroTime()->tm_year + 1900,

            _ => throw new NotImplementedException($"ResolveNumber: ExpressionType {expr.ExpressionType} ({(byte)expr.ExpressionType:X}) not implemented"),
        };
    }

    public static HaselSeString ResolveString(this BaseExpression expr, List<ExpressionWrapper>? localParameters = null)
    {
        if (expr is IntegerExpression integerExpression)
            return integerExpression.Value.ToString();

        if (expr is StringExpression stringExpression)
            return stringExpression.ResolveString(localParameters);

        if (expr is BinaryExpression binaryExpression)
            return binaryExpression.Resolve(localParameters) ? "1" : "0";

        if (expr is ParameterExpression parameterExpression)
            return parameterExpression.ResolveString(localParameters);

        return (expr.ExpressionType switch
        {
            ExpressionType.Millisecond => DateTime.Now.Millisecond,
            ExpressionType.Second => MacroDecoder.GetMacroTime()->tm_sec,
            ExpressionType.Minute => MacroDecoder.GetMacroTime()->tm_min,
            ExpressionType.Hour => MacroDecoder.GetMacroTime()->tm_hour,
            ExpressionType.Day => ((FixedTm*)MacroDecoder.GetMacroTime())->tm_mday,
            ExpressionType.Weekday => ((FixedTm*)MacroDecoder.GetMacroTime())->tm_wday,
            ExpressionType.Month => MacroDecoder.GetMacroTime()->tm_mon + 1,
            ExpressionType.Year => MacroDecoder.GetMacroTime()->tm_year + 1900,

            _ => throw new NotImplementedException($"ResolveNumber: ExpressionType {expr.ExpressionType} ({(byte)expr.ExpressionType:X}) not implemented"),
        }).ToString();
    }
}
