namespace HaselCommon.ImGuiYoga.Events;

public abstract class Event
{
    public Node? Sender { get; init; }
    public bool Bubbles { get; private set; } = true;

    public void StopPropagation()
    {
        Bubbles = false;
    }
}
