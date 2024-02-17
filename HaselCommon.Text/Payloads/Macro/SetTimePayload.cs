using FFXIVClientStructs.FFXIV.Component.Text;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.SetTime)] // n x
public class SetTimePayload : HaselMacroPayload
{
    public BaseExpression? Time { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override unsafe HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Time != null)
        {
            var mt = MacroDecoder.GetMacroTime();
            mt->SetTime(DateTimeOffset.FromUnixTimeSeconds(Time.ResolveNumber(localParameterData)).DateTime);
        }

        return this;
    }
}
