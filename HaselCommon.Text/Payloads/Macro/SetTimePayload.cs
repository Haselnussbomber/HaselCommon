using FFXIVClientStructs.FFXIV.Component.Text;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.SetTime)] // n x
public class SetTimePayload : MacroPayload
{
    public Expression? Time { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override unsafe SeString Resolve(List<Expression>? localParameters = null)
    {
        if (Time != null)
        {
            var mt = (FixedTm*)MacroDecoder.GetMacroTime();
            mt->SetTime(DateTimeOffset.FromUnixTimeSeconds(Time.ResolveNumber(localParameters)).DateTime.ToLocalTime());
        }

        return this;
    }
}
