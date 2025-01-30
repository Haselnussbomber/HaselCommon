using HaselCommon.Yoga.Events;

namespace HaselCommon.Yoga.Components.Events;

public class CheckboxStateChangeEvent : YogaEvent
{
    public bool IsChecked { get; set; }
}
