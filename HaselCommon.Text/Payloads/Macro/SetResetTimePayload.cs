using FFXIVClientStructs.FFXIV.Component.Text;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.SetResetTime)] // n N x
public class SetResetTimePayload : HaselMacroPayload
{
    public BaseExpression? Hour { get; set; }
    public BaseExpression? WeekDay { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override unsafe HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        // see Client::UI::Misc::RaptureTextModule___Component::Text::MacroDecoder_vf1
        // i just copy pasted and guessed

        var hour = (Hour?.ResolveNumber(localParameters) ?? 1) - 1;
        if (hour > 23) hour = 0;

        var day = (WeekDay?.ResolveNumber(localParameters) ?? 8) - 1;
        if (day > 7) day = 7;

        if (hour >= 9)
        {
            hour -= 9;
        }
        else
        {
            if (day != 7)
                day = (day + 6) % 7;

            hour += 15;
        }

        var mt = MacroDecoder.GetMacroTime();
        var now = DateTime.Now;
        mt->SetTime(now);

        if (day == 7)
        {
            var v14 = hour - now.Hour + mt->tm_hour;
            mt->tm_hour = v14;
            if (now.Hour >= hour)
                mt->tm_hour = v14 + 24;
        }
        else
        {
            var wday = (int)now.DayOfWeek;

            if (wday < day || (wday == day && now.Hour < hour))
                mt->tm_mday += day - wday;
            else
                mt->tm_mday += 7 + day - wday;
            mt->tm_hour += hour - now.Hour;
        }

        mt->tm_sec = 0;
        mt->tm_min = 0;

        return new();
    }
}
