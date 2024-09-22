using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using ExCSS;
using HaselCommon.ImGuiYoga.Elements;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

public static class NodeParser
{
    public record struct BorderResult(float[] LineWidth, HaselColor? Color);

    public static readonly StylesheetParser StylesheetParser = new(
        includeUnknownRules: true,
        includeUnknownDeclarations: true,
        tolerateInvalidSelectors: false,
        tolerateInvalidValues: true,
        tolerateInvalidConstraints: false,
        preserveComments: false,
        preserveDuplicateProperties: false);

    public static readonly IReadOnlyDictionary<string, Type> BuiltInTypes = new Dictionary<string, Type>() {
        { "div", typeof(Node) },
        { "span", typeof(Text) },
        { "header", typeof(HeaderElement) },
        { "footer", typeof(FooterElement) },
        { "section", typeof(SectionElement) },
        { "#text", typeof(Text) },
        { "br", typeof(BrElement) },
        { "a", typeof(AnchorElement) },
        { "icon", typeof(IconElement) },
        { "uld-part", typeof(UldPartElement) },
    };

    public static Node? ParseXmlNode(Document document, XmlNode xmlNode, Node? parentNode = null, XmlNode? templateUserNode = null, XmlAttributeCollection? templateParameters = null)
    {
        // document.Logger?.LogTrace("Processing {nodeType} {nodeName}", xmlNode.NodeType, xmlNode.Name);

        if (xmlNode.NodeType is XmlNodeType.Comment)
        {
            return new Comment(xmlNode.InnerText.Trim());
        }

        if (xmlNode.NodeType is XmlNodeType.Text)
        {
            // ignore empty nodes
            if (string.IsNullOrWhiteSpace(xmlNode.InnerText))
                return null;

            // handle {children} placeholder in template nodes
            if (templateUserNode != null && parentNode != null && xmlNode.InnerText.Trim() == "{children}")
            {
                foreach (XmlNode templateUserChild in templateUserNode.ChildNodes)
                {
                    var userChildNode = ParseXmlNode(document, templateUserChild, parentNode, templateUserNode, templateParameters);
                    if (userChildNode != null)
                        parentNode.Add(userChildNode);
                }
                return null;
            }

            // handle text in span tag
            if (parentNode is Text parentTextNode)
            {
                parentTextNode.Data = new ReadOnlySeString(Encoding.UTF8.GetBytes(xmlNode.InnerText.Trim())); // TODO: use ReadOnlySeString.FromText() when https://github.com/goatcorp/Dalamud/pull/2033 is merged
                return null;
            }

            // handle loose #text node
            if (parentNode != null)
            {
                return new Text(xmlNode.InnerText.Trim());
            }
        }

        if (xmlNode.NodeType is XmlNodeType.Element)
        {
            if (xmlNode.Name == "link" && xmlNode.Attributes!["rel"] is XmlAttribute relAttr && relAttr.Value == "stylesheet" && xmlNode.Attributes["href"] is XmlAttribute hrefAttr)
            {
                document.Stylesheets.AddEmbeddedResource(hrefAttr.Value);
                return null;
            }

            if (xmlNode.Name == "style") // TODO: scoped styles
            {
                document.Stylesheets.Add(xmlNode.InnerText);
                return null;
            }

            if (xmlNode.Name == "template")
            {
                if (xmlNode.Attributes == null || string.IsNullOrEmpty(xmlNode.Attributes["id"]?.Value))
                {
                    document.Logger?.LogError("Template is missing an id attribute.");
                    return null;
                }

                if (!xmlNode.HasChildNodes)
                {
                    document.Logger?.LogError("Template is missing a child nodes.");
                    return null;
                }

                document.Templates.Add(xmlNode.Attributes["id"]!.Value, xmlNode);
                return null;
            }
        }

        if (!document.CustomElements.TryGetValue(xmlNode.Name, out var yogaNodeType) && !BuiltInTypes.TryGetValue(xmlNode.Name, out yogaNodeType))
        {
            // apply template (cursed edition)
            if (xmlNode.NodeType is XmlNodeType.Element && document.Templates.TryGetValue(xmlNode.Name, out var templateNode) && parentNode != null)
            {
                var clonedTemplateNode = templateNode.CloneNode(true);
                templateUserNode = xmlNode;
                templateParameters = xmlNode.Attributes;

                foreach (XmlNode clonedTemplateChild in clonedTemplateNode.ChildNodes)
                {
                    var childNode = ParseXmlNode(document, clonedTemplateChild, parentNode, templateUserNode, templateParameters);
                    if (childNode != null)
                        parentNode.Add(childNode);
                }

                return null;
            }

            document.Logger?.LogError("Type {nodeName} is not registered", xmlNode.Name);
            return null;
        }

        var node = (Node?)Activator.CreateInstance(yogaNodeType);
        if (node == null)
        {
            document.Logger?.LogError("Could not instantiate type {nodeName}", xmlNode.Name);
            return null;
        }

        node.Parent = parentNode;
        node.ApplyXmlAttributes(templateParameters);
        node.ApplyXmlNode(xmlNode);

        foreach (XmlNode child in xmlNode.ChildNodes)
        {
            var childNode = ParseXmlNode(document, child, node, templateUserNode);
            if (childNode != null)
                node.Add(childNode);
        }

        return node;
    }

    public static HaselColor ParseStyleColor(string input)
    {
        if (input.StartsWith('#'))
        {
            var hex = input[1..];

            if (hex.Length == 3)
            {
                hex = "" + hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2] + "FF";
            }
            else if (hex.Length == 4)
            {
                hex = "" + hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2] + hex[3] + hex[3];
            }
            else if (hex.Length == 6)
            {
                hex += "FF";
            }
            else if (hex.Length == 8)
            {
                // good already
            }
            else
            {
                return HaselColor.From(0xFFFFFFFFu);
            }

            var red = uint.Parse(hex[0..2], NumberStyles.HexNumber);
            var green = uint.Parse(hex[2..4], NumberStyles.HexNumber);
            var blue = uint.Parse(hex[4..6], NumberStyles.HexNumber);
            var alpha = uint.Parse(hex[6..8], NumberStyles.HexNumber);

            return HaselColor.From(alpha << 24 | blue << 16 | green << 8 | red);
        }
        else if (input.StartsWith("rgb(") && input.EndsWith(')'))
        {
            var values = input[4..^1]
                .Split(',', StringSplitOptions.TrimEntries)
                .Select(uint.Parse)
                .ToArray();

            if (values.Length != 3)
                return HaselColor.From(0u);

            return HaselColor.From(0xFF000000 | values[2] << 16 | values[1] << 8 | values[0]);
        }
        else if (input.StartsWith("rgba(") && input.EndsWith(')'))
        {
            var values = input[5..^1]
                .Split(',', StringSplitOptions.TrimEntries)
                .Select(val => float.Parse(val, CultureInfo.InvariantCulture))
                .ToArray();

            if (values.Length != 4)
                return HaselColor.From(0u);

            return HaselColor.From((uint)(values[3] * 255f) << 24 | (uint)values[2] << 16 | (uint)values[1] << 8 | (uint)values[0]);
        }
        else if (input.StartsWith("imguicol(") && input.EndsWith(')'))
        {
            var name = input[9..^1];
            var values = Enum.GetValues<ImGuiCol>();
            foreach (var value in values)
            {
                if (Enum.GetName(value) == name)
                {
                    return HaselColor.From(value);
                }
            }
        }

        return HaselColor.From(0u);
    }

    // TODO: if none set, initial is passed
    public static BorderResult ParseStyleBorder(string value)
    {
        /*
            we don't support <line-style> (yet?), so format will be:

            border = <line-width> || <color>
            <line-width> = <length [0,∞]>
        */

        HaselColor? color = null;

        var colorStart = value.LastIndexOf('#');
        if (colorStart == -1)
            colorStart = value.LastIndexOf("rgb");
        if (colorStart != -1)
        {
            var colorvalue = value[colorStart..];
            value = value[..colorStart];
            color = ParseStyleColor(colorvalue);
        }

        var widths = value
            .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(float.Parse);

        return new BorderResult(widths.ToArray(), color);
    }

    public static YGValue ParseYGValue(string input)
    {
        var unit = YGUnit.Point;

        if (input.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            input = input[..^2].Trim();
        }
        else if (input.EndsWith("%", StringComparison.OrdinalIgnoreCase))
        {
            input = input[..^1].Trim();
            unit = YGUnit.Percent;
        }

        return new() { unit = unit, value = float.Parse(input) };
    }
}
