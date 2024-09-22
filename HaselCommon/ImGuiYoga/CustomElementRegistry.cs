using System.Collections.Generic;

namespace HaselCommon.ImGuiYoga;

// https://html.spec.whatwg.org/multipage/custom-elements.html#custom-elements-api
public class CustomElementRegistry : Dictionary<string, Type>
{
    public void Add<T>(string key) where T : Node
    {
        Add(key, typeof(T));
    }

    public new void Add(string key, Type value)
    {
        if (!value.IsAssignableTo(typeof(Node)))
            throw new ArgumentException("Type is not derived from Node.");

        base.Add(key, value);
    }
}
