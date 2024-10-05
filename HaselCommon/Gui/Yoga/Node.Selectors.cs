using HaselCommon.Gui.Yoga.Attributes;

namespace HaselCommon.Gui.Yoga;

public partial class Node
{
    [NodeProp("Node")]
    public virtual string TagName
    {
        get
        {
            if (GetType() == typeof(Node))
                return "Node";

            return GetType().FullName ?? GetType().Name;
        }
    }
}
