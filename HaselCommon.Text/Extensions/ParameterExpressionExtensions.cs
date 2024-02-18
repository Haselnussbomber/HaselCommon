using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;

namespace HaselCommon.Text.Extensions;

public static unsafe class ParameterExpressionExtensions
{
    public static int ResolveNumber(this ParameterExpression expr, List<ExpressionWrapper>? localParameters = null)
    {
        // gstr, gnum
        if (expr.ExpressionType is ExpressionType.ObjectParameter or ExpressionType.PlayerParameter)
        {
            var num = expr.Operand.ResolveNumber(localParameters) - 1;
            var param = RaptureTextModule.Instance()->TextModule.MacroDecoder.GlobalParameters.Get((ulong)num);

            return param.Type switch
            {
                TextParameterType.Uninitialized => 0,
                TextParameterType.Integer => param.IntValue,
                TextParameterType.Utf8String => param.Utf8StringValue->ToInteger(),
                TextParameterType.String => int.Parse(MemoryHelper.ReadStringNullTerminated((nint)param.StringValue)),
                _ => throw new NotImplementedException($"Unhandled ParameterDataType {(byte)param.Type:x02}"),
            };
        }

        // lstr, lnum
        if (expr.ExpressionType is ExpressionType.IntegerParameter or ExpressionType.StringParameter)
        {
            if (localParameters == null)
                throw new ArgumentException("No localParameters provided");

            var num = expr.Operand.ResolveNumber(localParameters) - 1;

            // TODO: silently return default (0)?
            if (num < 0)
                throw new ArgumentException($"Requiring invalid localParameters index {num}");

            if (localParameters.Count < num)
                throw new ArgumentException($"localParameters index {num} was not provided");

            return int.Parse(localParameters[num].ToString());
        }

        throw new NotImplementedException($"Unhandled ParameterExpression type 0x{(byte)expr.ExpressionType:X02}");
    }

    public static HaselSeString ResolveString(this ParameterExpression expr, List<ExpressionWrapper>? localParameters = null)
    {
        // gstr, gnum
        if (expr.ExpressionType is ExpressionType.ObjectParameter or ExpressionType.PlayerParameter)
        {
            var num = expr.Operand.ResolveNumber(localParameters) - 1;
            var param = RaptureTextModule.Instance()->TextModule.MacroDecoder.GlobalParameters.Get((ulong)num);

            return param.Type switch
            {
                TextParameterType.Uninitialized => "",
                TextParameterType.Integer => param.IntValue.ToString(),
                TextParameterType.Utf8String => HaselSeString.Parse(param.Utf8StringValue->StringPtr, param.Utf8StringValue->Length),
                TextParameterType.String => HaselSeString.Parse(MemoryHelper.ReadRawNullTerminated((nint)param.StringValue)),
                _ => throw new NotImplementedException($"Unhandled ParameterDataType {param.Type}"),
            };
        }

        // lstr, lnum
        if (expr.ExpressionType is ExpressionType.IntegerParameter or ExpressionType.StringParameter)
        {
            if (localParameters == null)
                throw new ArgumentException("No localParameters provided");

            var num = (int)uint.Parse(expr.Operand.ResolveString(localParameters).ToString()) - 1;

            // TODO: silently return default ("")?
            if (num < 0)
                throw new ArgumentException($"Requiring invalid localParameters index {num}");

            if (localParameters.Count < num)
                throw new ArgumentException($"localParameters index {num} was not provided");

            return localParameters[num].BaseExpression.ToString() ?? string.Empty;
        }

        throw new NotImplementedException($"Unhandled ParameterExpression type 0x{(byte)expr.ExpressionType:X02}");
    }
}
