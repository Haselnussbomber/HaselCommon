using System.Linq;

namespace HaselCommon.Text.Expressions;

/// <summary>
/// Represent an Expression containing SeString.
/// </summary>
public class StringExpression : Expression
{
    /// <summary>
    /// Construct a new StringExpression containing empty string.
    /// </summary>
    public StringExpression()
    {
        Value = new SeString();
    }

    /// <summary>
    /// Construct a new StringExpression containing the given value.
    /// </summary>
    public StringExpression(SeString value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// SeString contained in this expression.
    /// </summary>
    public SeString Value { get; }

    /// <inheritdoc />    
    public override int Size
    {
        get
        {
            var encodedLength = Value.Encode().Length;
            return 1 + IntegerExpression.CalculateSize((uint)encodedLength) + encodedLength;
        }
    }

    /// <inheritdoc />
    public override ExpressionType ExpressionType => ExpressionType.SeString;

    /// <inheritdoc />
    public override void Encode(Stream stream)
    {
        var encoded = Value.Encode();
        stream.WriteByte((byte)ExpressionType);
        IntegerExpression.EncodeStatic(stream, (uint)encoded.Length);
        stream.Write(encoded);
    }

    /// <inheritdoc />
    public override string ToString() => Value?.ToString() ?? string.Empty;

    /// <summary>
    /// Parse given Stream into a StringExpression.
    /// </summary>
    /// <param name="typeByte">Type marker byte.</param>
    /// <param name="stream">Stream to read from.</param>
    /// <returns>Parsed StringExpression.</returns>
    public static StringExpression Parse(byte typeByte, Stream stream)
    {
        if (typeByte != 0xFF)
            throw new ArgumentException($"Given type does not indicate a {nameof(StringExpression)}.", nameof(typeByte));

        var lengthExpression = IntegerExpression.Parse((byte)stream.ReadByte(), stream);
        var length = (int)lengthExpression.Value;
        var buffer = new byte[length];

        if (length != stream.Read(buffer, 0, length))
            throw new ArgumentException("Encountered premature end of input (unexpected EOF).", nameof(stream));

        if (buffer.Any(x => x == 0))
            throw new ArgumentException("Encountered premature end of input (unexpected null character).", nameof(stream));

        return length == 0
            ? new StringExpression()
            : new StringExpression(SeString.Parse(buffer));
    }

    public override int ResolveNumber(List<Expression>? localParameters = null)
    {
        return int.Parse(ResolveString(localParameters).ToString());
    }

    public override SeString ResolveString(List<Expression>? localParameters = null)
    {
        return Value.Resolve(localParameters);
    }
}
