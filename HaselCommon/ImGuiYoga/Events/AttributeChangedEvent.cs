namespace HaselCommon.ImGuiYoga.Events;

public class AttributeChangedEvent : Event
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}
