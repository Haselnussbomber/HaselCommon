using HaselCommon.ImGuiYoga.Events;

namespace HaselCommon.ImGuiYoga;

public partial class Node
{
    public override void DispatchEvent(Event evt)
    {
        base.DispatchEvent(evt);

        if (evt.Bubbles) // can be stopped by the nodes implementation of OnEvent through evt.StopPropagation()
        {
            Parent?.DispatchEvent(evt); // calls this function of the parent
        }
    }
}
