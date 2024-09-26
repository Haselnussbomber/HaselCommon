namespace HaselCommon.ImGuiYoga.Events;

public class StyleChangedEvent : Event
{
    public required string PropertyName { get; init; }
}
