using System.Collections.Generic;
using Dalamud.Memory;
using HaselCommon.Structs;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Extensions;

public static unsafe class ParameterExpressionExtensions
{
    public static int ResolveNumber(this ParameterExpression expr, List<HaselSeString>? localParameterData = null)
    {
        // gstr, gnum
        if (expr.ExpressionType is ExpressionType.ObjectParameter or ExpressionType.PlayerParameter)
        {
            var num = expr.Operand.ResolveNumber(localParameterData) - 1;
            var param = TextParameter.Get((ulong)num);

            return param.Type switch
            {
                TextParameterType.Uninitialized => 0,
                TextParameterType.Integer => param.IntValue,
                TextParameterType.Utf8StringPtr => param.Utf8StringValue->ToInteger(),
                TextParameterType.BytePtr => int.Parse(MemoryHelper.ReadStringNullTerminated((nint)param.BytePtrValue)),
                _ => throw new NotImplementedException($"Unhandled ParameterDataType {(byte)param.Type:x02}"),
            };
        }

        // lstr, lnum
        if (expr.ExpressionType is ExpressionType.IntegerParameter or ExpressionType.StringParameter)
        {
            if (localParameterData == null)
                throw new ArgumentException("No LocalParameterData provided");

            var num = expr.Operand.ResolveNumber(localParameterData) - 1;

            // TODO: silently return default (0)?
            if (num < 0)
                throw new ArgumentException($"Requiring invalid LocalParameterData index {num}");

            if (localParameterData.Count < num)
                throw new ArgumentException($"LocalParameterData index {num} was not provided");

            return int.Parse(localParameterData[num].ToString());
        }

        throw new NotImplementedException($"Unhandled ParameterExpression type 0x{(byte)expr.ExpressionType:X02}");
    }

    public static HaselSeString ResolveString(this ParameterExpression expr, List<HaselSeString>? localParameterData = null)
    {
        // gstr, gnum
        if (expr.ExpressionType is ExpressionType.ObjectParameter or ExpressionType.PlayerParameter)
        {
            var num = expr.Operand.ResolveNumber(localParameterData) - 1;
            var param = TextParameter.Get((ulong)num);

            return param.Type switch
            {
                TextParameterType.Uninitialized => "",
                TextParameterType.Integer => param.IntValue.ToString(),
                TextParameterType.Utf8StringPtr => HaselSeString.Parse(param.Utf8StringValue->StringPtr, param.Utf8StringValue->Length),
                TextParameterType.BytePtr => HaselSeString.Parse(MemoryHelper.ReadRawNullTerminated((nint)param.BytePtrValue)),
                _ => throw new NotImplementedException($"Unhandled ParameterDataType {param.Type}"),
            };
        }

        // lstr, lnum
        if (expr.ExpressionType is ExpressionType.IntegerParameter or ExpressionType.StringParameter)
        {
            if (localParameterData == null)
                throw new ArgumentException("No LocalParameterData provided");

            var num = (int)uint.Parse(expr.Operand.ResolveString(localParameterData).ToString()) - 1;

            // TODO: silently return default ("")?
            if (num < 0)
                throw new ArgumentException($"Requiring invalid LocalParameterData index {num}");

            if (localParameterData.Count < num)
                throw new ArgumentException($"LocalParameterData index {num} was not provided");

            return localParameterData[num];
        }

        throw new NotImplementedException($"Unhandled ParameterExpression type 0x{(byte)expr.ExpressionType:X02}");
    }
}
