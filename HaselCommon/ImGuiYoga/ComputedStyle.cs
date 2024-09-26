using System.Collections.Generic;
using HaselCommon.Enums;
using HaselCommon.ImGuiYoga.Events;
using HaselCommon.Utils;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

public unsafe class ComputedStyle(Node OwnerNode)
{
    // Yoga
    public float PositionTop => OwnerNode.YGNode->GetComputedTop();
    public float PositionLeft => OwnerNode.YGNode->GetComputedLeft();
    public float MarginTop => OwnerNode.YGNode->GetComputedMargin(YGEdge.Top);
    public float MarginRight => OwnerNode.YGNode->GetComputedMargin(YGEdge.Right);
    public float MarginBottom => OwnerNode.YGNode->GetComputedMargin(YGEdge.Bottom);
    public float MarginLeft => OwnerNode.YGNode->GetComputedMargin(YGEdge.Left);
    public float PaddingTop => OwnerNode.YGNode->GetComputedPadding(YGEdge.Top);
    public float PaddingRight => OwnerNode.YGNode->GetComputedPadding(YGEdge.Right);
    public float PaddingBottom => OwnerNode.YGNode->GetComputedPadding(YGEdge.Bottom);
    public float PaddingLeft => OwnerNode.YGNode->GetComputedPadding(YGEdge.Left);
    public float BorderTop => OwnerNode.YGNode->GetComputedBorder(YGEdge.Top);
    public float BorderRight => OwnerNode.YGNode->GetComputedBorder(YGEdge.Right);
    public float BorderBottom => OwnerNode.YGNode->GetComputedBorder(YGEdge.Bottom);
    public float BorderLeft => OwnerNode.YGNode->GetComputedBorder(YGEdge.Left);

    // Custom
    private HaselColor? _borderTopColor;
    public HaselColor BorderTopColor
    {
        get
        {
            if (_borderTopColor.HasValue)
                return _borderTopColor.Value;

            var value = OwnerNode.ResolveStyleValue("border-top-color");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _borderTopColor = Color;
                return _borderTopColor.Value;
            }

            _borderTopColor = NodeParser.ParseStyleColor(value);
            return _borderTopColor.Value;
        }
    }

    private HaselColor? _borderRightColor;
    public HaselColor BorderRightColor
    {
        get
        {
            if (_borderRightColor.HasValue)
                return _borderRightColor.Value;

            var value = OwnerNode.ResolveStyleValue("border-right-color");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _borderRightColor = Color;
                return _borderRightColor.Value;
            }

            _borderRightColor = NodeParser.ParseStyleColor(value);
            return _borderRightColor.Value;
        }
    }

    private HaselColor? _borderBottomColor;
    public HaselColor BorderBottomColor
    {
        get
        {
            if (_borderBottomColor.HasValue)
                return _borderBottomColor.Value;

            var value = OwnerNode.ResolveStyleValue("border-bottom-color");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _borderBottomColor = Color;
                return _borderBottomColor.Value;
            }

            _borderBottomColor = NodeParser.ParseStyleColor(value);
            return _borderBottomColor.Value;
        }
    }

    private HaselColor? _borderLeftColor;
    public HaselColor BorderLeftColor
    {
        get
        {
            if (_borderLeftColor.HasValue)
                return _borderLeftColor.Value;

            var value = OwnerNode.ResolveStyleValue("border-left-color");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _borderLeftColor = Color;
                return _borderLeftColor.Value;
            }

            _borderLeftColor = NodeParser.ParseStyleColor(value);
            return _borderLeftColor.Value;
        }
    }

    private float? _borderTopLeftRadius;
    public float BorderTopLeftRadius
    {
        get
        {
            if (_borderTopLeftRadius.HasValue)
                return _borderTopLeftRadius.Value;

            var value = OwnerNode.ResolveStyleValue("border-top-left-radius");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _borderTopLeftRadius = 0;
                return _borderTopLeftRadius.Value;
            }

            _borderTopLeftRadius = NodeParser.ParseFloat(value);
            return _borderTopLeftRadius.Value;
        }
    }

    private float? _borderTopRightRadius;
    public float BorderTopRightRadius
    {
        get
        {
            if (_borderTopRightRadius.HasValue)
                return _borderTopRightRadius.Value;

            var value = OwnerNode.ResolveStyleValue("border-top-right-radius");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _borderTopRightRadius = 0;
                return _borderTopRightRadius.Value;
            }

            _borderTopRightRadius = NodeParser.ParseFloat(value);
            return _borderTopRightRadius.Value;
        }
    }

    private float? _borderBottomLeftRadius;
    public float BorderBottomLeftRadius
    {
        get
        {
            if (_borderBottomLeftRadius.HasValue)
                return _borderBottomLeftRadius.Value;

            var value = OwnerNode.ResolveStyleValue("border-bottom-left-radius");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _borderBottomLeftRadius = 0;
                return _borderBottomLeftRadius.Value;
            }

            _borderBottomLeftRadius = NodeParser.ParseFloat(value);
            return _borderBottomLeftRadius.Value;
        }
    }

    private float? _borderBottomRightRadius;
    public float BorderBottomRightRadius
    {
        get
        {
            if (_borderBottomRightRadius.HasValue)
                return _borderBottomRightRadius.Value;

            var value = OwnerNode.ResolveStyleValue("border-bottom-right-radius");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _borderBottomRightRadius = 0;
                return _borderBottomRightRadius.Value;
            }

            _borderBottomRightRadius = NodeParser.ParseFloat(value);
            return _borderBottomRightRadius.Value;
        }
    }

    private HaselColor? _color;
    public HaselColor Color
    {
        get
        {
            if (_color.HasValue)
                return _color.Value;

            var value = OwnerNode.ResolveStyleValue("color");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _color = HaselColor.From(ImGuiCol.Text);
                return _color.Value;
            }

            _color = NodeParser.ParseStyleColor(value);
            return _color.Value;
        }
    }

    private Cursor? _cursor;
    public Cursor Cursor
    {
        get
        {
            if (_cursor.HasValue)
                return _cursor.Value;

            var value = OwnerNode.ResolveStyleValue("cursor");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _cursor = Cursor.Pointer;
                return _cursor.Value;
            }

            _cursor = value switch
            {
                "hand" => Cursor.Hand,
                "not-allowed" => Cursor.NotAllowed,
                "text" => Cursor.Text,
                "ns-resize" => Cursor.ResizeNS,
                "ew-resize" => Cursor.ResizeEW,
                _ => Cursor.Pointer
            };

            return _cursor.Value;
        }
    }

    private string? _fontName;
    public string FontName
    {
        get
        {
            if (!string.IsNullOrEmpty(_fontName))
                return _fontName;

            var value = OwnerNode.ResolveStyleValue("font-name");

            if (string.IsNullOrEmpty(value) || value == "initial")
                return _fontName = "axis";

            return _fontName = value;
        }
    }

    private float? _fontSize;
    public float FontSize
    {
        get
        {
            if (_fontSize.HasValue)
                return _fontSize.Value;

            var value = OwnerNode.ResolveStyleValue("font-size");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _fontSize = ImGui.GetFontSize();
                return _fontSize.Value;
            }

            _fontSize = NodeParser.ParseFloat(value);
            return _fontSize.Value;
        }
    }


    private HaselColor? _backgroundColor;
    public HaselColor BackgroundColor
    {
        get
        {
            if (_backgroundColor.HasValue)
                return _backgroundColor.Value;

            var value = OwnerNode.ResolveStyleValue("background-color");

            if (string.IsNullOrEmpty(value) || value == "initial")
            {
                _backgroundColor = Colors.Transparent;
                return _backgroundColor.Value;
            }

            _backgroundColor = NodeParser.ParseStyleColor(value);
            return _backgroundColor.Value;
        }
    }

    internal void ResetCache()
    {
        // reset compute-on-demand caches
        _borderTopColor = null;
        _borderRightColor = null;
        _borderBottomColor = null;
        _borderLeftColor = null;
        _borderTopLeftRadius = null;
        _borderTopRightRadius = null;
        _borderBottomLeftRadius = null;
        _borderBottomRightRadius = null;
        _color = null;
        _cursor = null;
        _fontName = null;
        _fontSize = null;
        _backgroundColor = null;

        // re-apply styles
        var propertyNames = new HashSet<string>();

        foreach (var (_, declaration) in OwnerNode.StyleDeclarations)
        {
            foreach (var attr in declaration)
            {
                propertyNames.Add(attr.Name);
            }
        }

        foreach (var attr in OwnerNode.Style)
        {
            propertyNames.Add(attr.Key);
        }

        foreach (var propertyName in propertyNames)
        {
            OwnerNode.DispatchEvent(new StyleChangedEvent()
            {
                PropertyName = propertyName
            });
        }
    }
}
