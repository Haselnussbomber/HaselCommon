using HaselCommon.Gui.Yoga.Events;

namespace HaselCommon.Gui.Yoga.Components.Events;

public class CheckboxStateChangeEvent : YogaEvent
{
    public bool IsChecked { get; set; }
}
