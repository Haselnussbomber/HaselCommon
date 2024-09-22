using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ExCSS;
using Microsoft.Extensions.Logging;

namespace HaselCommon.ImGuiYoga;

public class Document : Node
{
    internal Dictionary<string, XmlNode> Templates { get; } = []; // key is id
    public CustomElementRegistry CustomElements { get; } = [];
    public Stylesheets Stylesheets { get; } = [];
    public ILogger? Logger { get; set; } // TODO: make this a requirement?
    private bool IsStyleDirty { get; set; } = true;
    public void SetStyleDirty() => IsStyleDirty = true;

    public Node CreateElement(string tagName)
    {
        return CreateElement<Node>(tagName);
    }

    public T CreateElement<T>(string tagName) where T : Node
    {
        if (!CustomElements.TryGetValue(tagName, out var type) && !NodeParser.BuiltInTypes.TryGetValue(tagName, out type))
            throw new InvalidOperationException($"Type with tag {tagName} is not registered.");

        return (T)Activator.CreateInstance(type)!;
    }

    public Text CreateTextElement()
    {
        return new Text();
    }

    public override void Update()
    {
        if (IsStyleDirty)
        {
            Logger?.LogTrace("Updating styles...");

            var map = new Dictionary<Node, HashSet<IStyleRule>>();

            foreach (var stylesheet in Stylesheets)
            {
                foreach (var styleRule in stylesheet.StyleRules)
                {
                    Logger?.LogTrace("Processing StyleRule {selectorTypeName} {rule}", styleRule.Selector.GetType().Name, styleRule.ToCss());

                    if (Matches(styleRule.Selector))
                    {
                        if (!map.TryGetValue(this, out var ruleset))
                            map.Add(this, ruleset = []);

                        ruleset.Add(styleRule);
                    }

                    foreach (var node in QuerySelectorAll(styleRule.Selector))
                    {
                        if (!map.TryGetValue(node, out var ruleset))
                            map.Add(node, ruleset = []);

                        ruleset.Add(styleRule);
                    }
                }
            }

            foreach (var (node, ruleset) in map)
            {
                try
                {
                    node.Style.Reset();

                    // TODO: this doesn't respect !important at all

                    foreach (var rules in ruleset.OrderBy(rule => rule.Selector.Specificity))
                    {
                        Logger?.LogTrace("Applying StyleRule {rule} to {displayName}", rules.ToCss(), node.DisplayName);

                        node.Style.Apply(rules.Style);
                    }

                    node.Style.ApplyStyleAttribute();
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Unexpected error on applying styles");
                }
            }

            IsStyleDirty = false;
        }

        base.Update();
    }
}
