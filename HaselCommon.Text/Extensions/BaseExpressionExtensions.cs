using System.Collections.Generic;
using HaselCommon.Structs;
using HaselCommon.Text.Enums;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Extensions;

// ReadExpression: "E8 ?? ?? ?? ?? 0F B7 55 18"
// ReadParameter: "E8 ?? ?? ?? ?? 49 8B 45 68"
// ReadPackedInteger (0xF0-0xFE): "0F B6 01 4C 8D 49 01 05"

public static unsafe partial class BaseExpressionExtensions
{
    // TODO: remove once lumina is updated
    public static string UpdatedToString(this BaseExpression expr)
    {
        if (expr is StringExpression stringExpression)
            return stringExpression.Value;

        if (expr is PlaceholderExpression placeholderExpression)
        {
            return (UpdatedExpressionType)placeholderExpression.ExpressionType switch
            {
                UpdatedExpressionType.Millisecond => "t_msec",
                UpdatedExpressionType.Second => "t_sec",
                UpdatedExpressionType.Minute => "t_min",
                UpdatedExpressionType.Hour => "t_hour",
                UpdatedExpressionType.Day => "t_day",
                UpdatedExpressionType.Weekday => "t_wday",
                UpdatedExpressionType.Month => "t_mon",
                UpdatedExpressionType.Year => "t_year",
                UpdatedExpressionType.StackColor => "stackcolor",
                _ => $"Placeholder#{(byte)expr.ExpressionType:X02}"
            };
        }

        if (expr is ParameterExpression parameterExpression)
        {
            return (UpdatedExpressionType)parameterExpression.ExpressionType switch
            {
                UpdatedExpressionType.IntegerParameter => $"lnum{parameterExpression.Operand.UpdatedToString()}",
                UpdatedExpressionType.PlayerParameter => $"gnum{parameterExpression.Operand.UpdatedToString()}",
                UpdatedExpressionType.StringParameter => $"lstr{parameterExpression.Operand.UpdatedToString()}",
                UpdatedExpressionType.ObjectParameter => $"gstr{parameterExpression.Operand.UpdatedToString()}",
                _ => throw new NotImplementedException()
            };
        }

        if (expr is BinaryExpression binaryExpression)
        {
            var Operand1 = binaryExpression.Operand1.UpdatedToString();
            var Operand2 = binaryExpression.Operand2.UpdatedToString();
            return (UpdatedExpressionType)binaryExpression.ExpressionType switch
            {
                UpdatedExpressionType.GreaterThanOrEqualTo => $"[{Operand1}>={Operand2}]",
                UpdatedExpressionType.GreaterThan => $"[{Operand1}>{Operand2}]",
                UpdatedExpressionType.LessThanOrEqualTo => $"[{Operand1}<={Operand2}]",
                UpdatedExpressionType.LessThan => $"[{Operand1}<{Operand2}]",
                UpdatedExpressionType.Equal => $"[{Operand1}=={Operand2}]",
                UpdatedExpressionType.NotEqual => $"[{Operand1}!={Operand2}]",
                _ => throw new NotImplementedException()
            };
        }

        return expr.ToString()!;
    }

    public static int ResolveNumber(this BaseExpression expr, List<HaselSeString>? localParameterData = null)
    {
        if (expr is IntegerExpression integerExpression)
            return (int)integerExpression.Value;

        if (expr is StringExpression stringExpression)
            return stringExpression.ResolveNumber(localParameterData);

        if (expr is BinaryExpression binaryExpression)
            return binaryExpression.Resolve(localParameterData) ? 1 : 0;

        if (expr is ParameterExpression parameterExpression)
            return parameterExpression.ResolveNumber(localParameterData);

        return (UpdatedExpressionType)expr.ExpressionType switch
        {
            UpdatedExpressionType.Millisecond => DateTime.Now.Millisecond,
            UpdatedExpressionType.Second => MacroTime.Instance()->tm_sec,
            UpdatedExpressionType.Minute => MacroTime.Instance()->tm_min,
            UpdatedExpressionType.Hour => MacroTime.Instance()->tm_hour,
            UpdatedExpressionType.Day => MacroTime.Instance()->tm_mday + 1,
            UpdatedExpressionType.Weekday => MacroTime.Instance()->tm_wday + 1,
            UpdatedExpressionType.Month => MacroTime.Instance()->tm_mon + 1,
            UpdatedExpressionType.Year => MacroTime.Instance()->tm_year + 1900,

            _ => throw new NotImplementedException($"ResolveNumber: ExpressionType {expr.ExpressionType} ({(byte)expr.ExpressionType:X}) not implemented"),
        };
    }

    public static HaselSeString ResolveString(this BaseExpression expr, List<HaselSeString>? localParameterData = null)
    {
        if (expr is IntegerExpression integerExpression)
            return integerExpression.Value.ToString();

        if (expr is StringExpression stringExpression)
            return stringExpression.ResolveString(localParameterData);

        if (expr is BinaryExpression binaryExpression)
            return binaryExpression.Resolve(localParameterData) ? "1" : "0";

        if (expr is ParameterExpression parameterExpression)
            return parameterExpression.ResolveString(localParameterData);

        return (UpdatedExpressionType)expr.ExpressionType switch
        {
            UpdatedExpressionType.Millisecond => DateTime.Now.Millisecond.ToString(),
            UpdatedExpressionType.Second => MacroTime.Instance()->tm_sec.ToString(),
            UpdatedExpressionType.Minute => MacroTime.Instance()->tm_min.ToString(),
            UpdatedExpressionType.Hour => MacroTime.Instance()->tm_hour.ToString(),
            UpdatedExpressionType.Day => (MacroTime.Instance()->tm_mday + 1).ToString(),
            UpdatedExpressionType.Weekday => (MacroTime.Instance()->tm_wday + 1).ToString(),
            UpdatedExpressionType.Month => (MacroTime.Instance()->tm_mon + 1).ToString(),
            UpdatedExpressionType.Year => (MacroTime.Instance()->tm_year + 1900).ToString(),

            _ => throw new NotImplementedException($"ResolveNumber: ExpressionType {expr.ExpressionType} ({(byte)expr.ExpressionType:X}) not implemented"),
        };
    }
}
