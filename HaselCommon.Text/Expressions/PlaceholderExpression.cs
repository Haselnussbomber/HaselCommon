using FFXIVClientStructs.FFXIV.Component.Text;

namespace HaselCommon.Text.Expressions;

// https://github.com/NotAdam/Lumina/blob/master/src/Lumina/Text/Expressions/PlaceholderExpression.cs

/// <summary>
/// Represent an Expression containing a placeholder value.
/// </summary>
public class PlaceholderExpression : Expression
{
    /// <summary>
    /// Construct a new StringExpression containing the given value.
    /// </summary>
    public PlaceholderExpression(ExpressionType expressionType)
    {
        if (!IsPlaceholderType(expressionType))
            throw new ArgumentException($"Given value does not indicate an {nameof(PlaceholderExpression)}.", nameof(expressionType));
        ExpressionType = expressionType;
    }

    /// <inheritdoc />
    public override int Size => 1;

    /// <inheritdoc />
    public override ExpressionType ExpressionType { get; }

    /// <inheritdoc />
    public override void Encode(Stream stream) => stream.WriteByte((byte)ExpressionType);

    /// <inheritdoc />
    public override string ToString()
    {
        return ExpressionType switch
        {
            ExpressionType.Millisecond => "t_msec",
            ExpressionType.Second => "t_sec",
            ExpressionType.Minute => "t_min",
            ExpressionType.Hour => "t_hour",
            ExpressionType.Day => "t_day",
            ExpressionType.Weekday => "t_wday",
            ExpressionType.Month => "t_mon",
            ExpressionType.Year => "t_year",
            ExpressionType.StackColor => "stackcolor",
            _ => $"Placeholder#{(byte)ExpressionType:X02}"
        };
    }

    /// <summary>
    /// Identify whether the given type byte indicates a PlaceholderExpression.
    /// </summary>
    /// <param name="typeByte">Type byte to identify.</param>
    /// <returns>True if it indicates a PlaceholderExpression.</returns>
    public static bool IsPlaceholderType(ExpressionType typeByte)
    {
        return (byte)typeByte switch
        {
            (>= 0xD0 and <= 0xDF) or 0xEC => true,
            _ => false,
        };
    }

    /// <summary>
    /// Parse given Stream into a PlaceholderExpression.
    /// </summary>
    /// <param name="typeByte">Type marker byte.</param>
    /// <param name="br">Stream to read from.</param>
    /// <returns>Parsed PlaceholderExpression.</returns>
    public static PlaceholderExpression Parse(byte typeByte, Stream stream) => new((ExpressionType)typeByte);

    public override unsafe int ResolveNumber(List<Expression>? localParameters = null)
    {
        return ExpressionType switch
        {
            ExpressionType.Millisecond => DateTime.Now.Millisecond,
            ExpressionType.Second => MacroDecoder.GetMacroTime()->tm_sec,
            ExpressionType.Minute => MacroDecoder.GetMacroTime()->tm_min,
            ExpressionType.Hour => MacroDecoder.GetMacroTime()->tm_hour,
            ExpressionType.Day => MacroDecoder.GetMacroTime()->tm_mday,
            ExpressionType.Weekday => MacroDecoder.GetMacroTime()->tm_wday,
            ExpressionType.Month => MacroDecoder.GetMacroTime()->tm_mon + 1,
            ExpressionType.Year => MacroDecoder.GetMacroTime()->tm_year + 1900,

            _ => throw new NotImplementedException($"ExpressionType {ExpressionType} ({(byte)ExpressionType:X}) not implemented"),
        };
    }

    public override SeString ResolveString(List<Expression>? localParameters = null)
    {
        return ResolveNumber(localParameters).ToString();
    }
}
