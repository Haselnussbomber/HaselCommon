using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;

namespace HaselCommon.Text.Expressions;

// https://github.com/NotAdam/Lumina/blob/master/src/Lumina/Text/Expressions/ParameterExpression.cs

/// <summary>
/// Represent an Expression containing a parameter with one argument(operand).
/// </summary>
public class ParameterExpression : Expression
{
    /// <summary>
    /// Construct a new ParameterExpression with given type and operand.
    /// </summary>
    public ParameterExpression(ExpressionType typeByte, Expression operand)
    {
        if (!IsParameterType(typeByte))
            throw new ArgumentException($"Given value does not indicate a {nameof(ParameterExpression)}.", nameof(typeByte));
        ExpressionType = typeByte;
        Operand = operand;
    }

    /// <summary>
    /// The operand.
    /// </summary>
    public Expression Operand { get; }

    /// <inheritdoc />
    public override int Size => 1 + Operand.Size;

    /// <inheritdoc />
    public override ExpressionType ExpressionType { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return ExpressionType switch
        {
            ExpressionType.IntegerParameter => $"lnum{Operand}",
            ExpressionType.PlayerParameter => $"gnum{Operand}",
            ExpressionType.StringParameter => $"lstr{Operand}",
            ExpressionType.ObjectParameter => $"gstr{Operand}",
            _ => throw new NotImplementedException() // cannot reach, as this instance is immutable and this field is filtered from constructor
        };
    }

    /// <summary>
    /// Identify whether the given type byte indicates an ParameterExpression.
    /// </summary>
    /// <param name="typeByte">Type byte to identify.</param>
    /// <returns>True if it indicates a ParameterExpression.</returns>
    public static bool IsParameterType(ExpressionType typeByte)
    {
        return (byte)typeByte switch
        {
            >= 0xE8 and <= 0xEB => true,
            _ => false
        };
    }

    /// <inheritdoc />
    public override void Encode(Stream stream)
    {
        stream.WriteByte((byte)ExpressionType);
        Operand.Encode(stream);
    }

    /// <summary>
    /// Parse given Stream into a ParameterExpression.
    /// </summary>
    /// <param name="typeByte">Type marker byte.</param>
    /// <param name="stream">Stream to read from.</param>
    /// <returns>Parsed ParameterExpression.</returns>
    public static ParameterExpression Parse(byte typeByte, Stream stream)
    {
        var operand = Expression.Parse(stream);
        return new ParameterExpression((ExpressionType)typeByte, operand);
    }

    public override unsafe int ResolveNumber(List<Expression>? localParameters = null)
    {
        // gstr, gnum
        if (ExpressionType is ExpressionType.ObjectParameter or ExpressionType.PlayerParameter)
        {
            var num = Operand.ResolveNumber(localParameters) - 1;
            var param = RaptureTextModule.Instance()->TextModule.MacroDecoder.GlobalParameters.Get((ulong)num);

            return param.Type switch
            {
                TextParameterType.Uninitialized => 0,
                TextParameterType.Integer => param.IntValue,
                TextParameterType.ReferencedUtf8String => param.ReferencedUtf8StringValue->Utf8String.ToInteger(),
                TextParameterType.String => int.Parse(MemoryHelper.ReadStringNullTerminated((nint)param.StringValue)),
                _ => throw new NotImplementedException($"Unhandled ParameterDataType {(byte)param.Type:x02}"),
            };
        }

        // lstr, lnum
        if (ExpressionType is ExpressionType.IntegerParameter or ExpressionType.StringParameter)
        {
            if (localParameters == null)
                return 0;

            var num = Operand.ResolveNumber(localParameters) - 1;

            if (num < 0 || localParameters.Count < num)
                return 0;

            return int.Parse(localParameters[num].ToString() ?? "0");
        }

        throw new NotImplementedException($"Unhandled ParameterExpression type 0x{(byte)ExpressionType:X02}");
    }

    public override unsafe SeString ResolveString(List<Expression>? localParameters = null)
    {
        // gstr, gnum
        if (ExpressionType is ExpressionType.ObjectParameter or ExpressionType.PlayerParameter)
        {
            var num = Operand.ResolveNumber(localParameters) - 1;
            var param = RaptureTextModule.Instance()->TextModule.MacroDecoder.GlobalParameters.Get((ulong)num);

            return param.Type switch
            {
                TextParameterType.Uninitialized => "",
                TextParameterType.Integer => param.IntValue.ToString(),
                TextParameterType.ReferencedUtf8String => SeString.Parse(param.ReferencedUtf8StringValue->Utf8String.AsSpan()),
                TextParameterType.String => SeString.Parse(MemoryHelper.ReadRawNullTerminated((nint)param.StringValue)),
                _ => throw new NotImplementedException($"Unhandled ParameterDataType {param.Type}"),
            };
        }

        // lstr, lnum
        if (ExpressionType is ExpressionType.IntegerParameter or ExpressionType.StringParameter)
        {
            if (localParameters == null)
                return string.Empty;

            var num = (int)uint.Parse(Operand.ResolveString(localParameters).ToString()) - 1;

            if (num < 0 || localParameters.Count < num)
                return string.Empty;

            if (localParameters[num] is StringExpression stringExpression)
                return stringExpression.Value;

            return localParameters[num].ToString() ?? string.Empty;
        }

        throw new NotImplementedException($"Unhandled ParameterExpression type 0x{(byte)ExpressionType:X02}");
    }
}
