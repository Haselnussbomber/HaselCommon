using FFXIVClientStructs.FFXIV.Component.Text;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.SetResetTime)] // n N x
public class SetResetTimePayload : HaselMacroPayload
{
    public ExpressionWrapper? Hour { get; set; }
    public ExpressionWrapper? WeekDay { get; set; }

    [TerminatorExpression]
    private ExpressionWrapper? Terminator { get; set; }

    public override unsafe HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        // not quite what Client::UI::Misc::RaptureTextModule___Component::Text::MacroDecoder_vf1 does
        // but it seems to work?!

        var hour = Hour?.ResolveNumber(localParameters) ?? 0;
        var weekday = WeekDay?.ResolveNumber(localParameters) ?? 7;

        var weekdayDate = DateTime.Now.AddDays((weekday - (int)DateTime.Now.DayOfWeek + 7) % 7);
        var date = new DateTime(weekdayDate.Year, weekdayDate.Month, weekdayDate.Day, hour, 0, 0, DateTimeKind.Utc).ToLocalTime();

        var mt = (FixedTm*)MacroDecoder.GetMacroTime();
        mt->SetTime(date);

        return new();
    }
}
