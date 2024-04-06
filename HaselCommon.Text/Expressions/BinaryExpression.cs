namespace HaselCommon.Text.Expressions;

// https://github.com/NotAdam/Lumina/blob/master/src/Lumina/Text/Expressions/BinaryExpression.cs

/// <summary>
/// Represent an Expression containing an operator with two arguments.
/// </summary>
public class BinaryExpression : Expression
{
    /// <summary>
    /// Construct a new UnaryExpression with given type and operand.
    /// </summary>
    public BinaryExpression(ExpressionType typeByte, Expression operand1, Expression operand2)
    {
        if (!IsBinaryType(typeByte))
            throw new ArgumentException($"Given value does not indicate an {nameof(ParameterExpression)}.", nameof(typeByte));
        ExpressionType = typeByte;
        Operand1 = operand1;
        Operand2 = operand2;
    }

    /// <summary>
    /// The first operand.
    /// </summary>
    public Expression Operand1 { get; }

    /// <summary>
    /// The second operand.
    /// </summary>
    public Expression Operand2 { get; }

    /// <inheritdoc />
    public override int Size => 1 + Operand1.Size + Operand2.Size;

    /// <inheritdoc />
    public override ExpressionType ExpressionType { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return ExpressionType switch
        {
            ExpressionType.GreaterThanOrEqualTo => $"[{Operand1}>={Operand2}]",
            ExpressionType.GreaterThan => $"[{Operand1}>{Operand2}]",
            ExpressionType.LessThanOrEqualTo => $"[{Operand1}<={Operand2}]",
            ExpressionType.LessThan => $"[{Operand1}<{Operand2}]",
            ExpressionType.Equal => $"[{Operand1}=={Operand2}]",
            ExpressionType.NotEqual => $"[{Operand1}!={Operand2}]",
            _ => throw new NotImplementedException() // cannot reach, as this instance is immutable and this field is filtered from constructor
        };
    }

    /// <summary>
    /// Identify whether the given type byte indicates an UnaryExpression.
    /// </summary>
    /// <param name="typeByte">Type byte to identify.</param>
    /// <returns>True if it indicates an UnaryExpression.</returns>
    public static bool IsBinaryType(ExpressionType typeByte)
    {
        return (byte)typeByte switch
        {
            >= 0xE0 and <= 0xE5 => true,
            _ => false
        };
    }

    /// <inheritdoc />
    public override void Encode(Stream stream)
    {
        stream.WriteByte((byte)ExpressionType);
        Operand1.Encode(stream);
        Operand2.Encode(stream);
    }

    /// <summary>
    /// Parse given Stream into a BinaryExpression.
    /// </summary>
    /// <param name="typeByte">Type marker byte.</param>
    /// <param name="stream">Stream to read from.</param>
    /// <returns>Parsed BinaryExpression.</returns>
    public static BinaryExpression Parse(byte typeByte, Stream stream)
    {
        var operand1 = Expression.Parse(stream);
        var operand2 = Expression.Parse(stream);
        return new BinaryExpression((ExpressionType)typeByte, operand1, operand2);
    }

    public bool Resolve(List<Expression>? localParameters = null)
    {
        if (ExpressionType is ExpressionType.Equal or ExpressionType.NotEqual)
        {
            var op1 = Operand1.ResolveString(localParameters);
            var op2 = Operand2.ResolveString(localParameters);

            return ExpressionType == ExpressionType.Equal
                ? op1 == op2
                : op1 != op2;
        }
        else
        {
            var op1 = Operand1.ResolveNumber(localParameters);
            var op2 = Operand2.ResolveNumber(localParameters);

            return ExpressionType switch
            {
                ExpressionType.GreaterThanOrEqualTo => op1 >= op2,
                ExpressionType.GreaterThan => op1 > op2,
                ExpressionType.LessThanOrEqualTo => op1 <= op2,
                ExpressionType.LessThan => op1 < op2,
                _ => throw new NotImplementedException($"Unhandled BinaryExpression type 0x{(byte)ExpressionType:X02}"),
            };
        }
    }

    public override int ResolveNumber(List<Expression>? localParameters = null)
    {
        return Resolve(localParameters) ? 1 : 0;
    }

    public override SeString ResolveString(List<Expression>? localParameters = null)
    {
        return Resolve(localParameters) ? "1" : "0";
    }
}
