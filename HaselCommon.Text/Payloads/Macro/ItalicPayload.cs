namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Italic)] // n
public class ItalicPayload : MacroPayload
{
    public ItalicPayload() : base()
    {
    }

    public ItalicPayload(bool enabled) : base()
    {
        Enabled = enabled ? 1u : 0;
    }

    public Expression Enabled { get; set; } = 0;
}
