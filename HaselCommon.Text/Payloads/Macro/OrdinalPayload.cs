namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Ordinal)] // n x
public class OrdinalPayload : MacroPayload
{
    public Expression? Value { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Value == null)
            return new();

        //! https://stackoverflow.com/a/20175

        var value = Value?.ResolveNumber(localParameters) ?? 0;

        if (value <= 0)
            return value.ToString();

        switch (value % 100)
        {
            case 11:
            case 12:
            case 13:
                return $"{value}th";
        }

        switch (value % 10)
        {
            case 1:
                return $"{value}st";
            case 2:
                return $"{value}nd";
            case 3:
                return $"{value}rd";
            default:
                return $"{value}th";
        }
    }
}
