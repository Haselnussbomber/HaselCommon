namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Bold)] // n
public class BoldPayload : HaselMacroPayload
{
    public BoldPayload() : base()
    {
    }

    public BoldPayload(bool enabled) : base()
    {
        Enabled = new(enabled ? 1u : 0);
    }

    public IntegerExpression Enabled { get; set; } = new();
}
