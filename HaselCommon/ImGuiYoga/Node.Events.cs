using System.Reflection;
using HaselCommon.ImGuiYoga.Attributes;
using HaselCommon.ImGuiYoga.Events;
using Microsoft.Extensions.Logging;

namespace HaselCommon.ImGuiYoga;

public partial class Node
{
    public override void DispatchEvent(Event evt)
    {
        if (evt is ChildrenChangedEvent)
            UpdateRefsPending = true;

        base.DispatchEvent(evt);

        if (evt.Bubbles) // can be stopped by the nodes implementation of OnEvent through evt.StopPropagation()
        {
            Parent?.DispatchEvent(evt); // calls this function of the parent
        }
    }

    private void UpdateRefs()
    {
        GetDocument()?.Logger?.LogDebug("Updating refs of {node}", Guid.ToString());

        foreach (var propInfo in CachedType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (propInfo.GetCustomAttribute<NodeRefAttribute>() is NodeRefAttribute refAttr)
            {
                propInfo.SetValue(this, QuerySelector(refAttr.Selector));
            }
        }

        foreach (var fieldInfo in CachedType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (fieldInfo.GetCustomAttribute<NodeRefAttribute>() is NodeRefAttribute refAttr)
            {
                fieldInfo.SetValue(this, QuerySelector(refAttr.Selector));
            }
        }
    }
}
