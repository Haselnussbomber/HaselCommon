namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Italic)] // n
public class ItalicPayload : HaselMacroPayload
{
    public ItalicPayload() : base()
    {
    }

    public ItalicPayload(bool enabled) : base()
    {
        Enabled = new(enabled ? 1u : 0);
    }

    public IntegerExpression Enabled { get; set; } = new();
}
