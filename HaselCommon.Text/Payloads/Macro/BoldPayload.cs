namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Bold)] // n
public class BoldPayload : HaselMacroPayload
{
    public BoldPayload() : base()
    {
    }

    public BoldPayload(bool enabled) : base()
    {
        Enabled = enabled ? 1u : 0;
    }

    public ExpressionWrapper Enabled { get; set; } = 0;
}
