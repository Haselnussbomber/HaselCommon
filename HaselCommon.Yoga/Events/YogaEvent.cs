namespace HaselCommon.Yoga.Events;

public class YogaEvent : EventArgs
{
    public bool Bubbles { get; set; } = true;
}
