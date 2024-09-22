using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Xml;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.ImGuiYoga.Attributes;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

public unsafe partial class Node : EventTarget
{
    public Guid Guid { get; } = Guid.NewGuid();
    public string DisplayName => !string.IsNullOrEmpty(Id) ? Id : Guid.ToString();

    private Type CachedType { get; }
    public string TypeName => CachedType.Name;
    public bool IsDebugHovered { get; set; }
    public bool IsSetupComplete { get; private set; }

    public Node? Parent { get; internal set; } // set when attached as child to another node

    public NodeStyle Style { get; }
    public DOMTokenList ClassList { get; }
    public NamedNodeMap Attributes { get; }

    [NodeProperty("id")]
    public string Id
    {
        get => Attributes["id"] ?? string.Empty;
        set => Attributes["id"] = value;
    }

    [NodeProperty("class")]
    public string ClassName
    {
        get => Attributes["class"] ?? string.Empty;
        set => Attributes["class"] = value;
    }

    // Interactables
    [NodeProperty("enableMouse")]
    public bool EnableMouse { get; set; }
    public bool IsHovered { get; private set; } = false;

    private bool IsDisposed;
    private readonly ImRaii.Style ChildFrameStyle = new();
    private readonly ImRaii.Color ChildFrameColor = new();
    private GCHandle? MeasureFuncHandle;
    private Vector2 InteractableLastMousePos = Vector2.Zero;

    #region for react reconciler

    private readonly Dictionary<string, dynamic> Props = [];

    public void SetProp(string key, dynamic value)
    {
        Props[key] = value;

        switch (key)
        {
            case "id":
                Id = (string)value;
                break;

            case "className":
                ClassName = (string)value;
                break;
        }
    }

    public void UnsetProp(string key)
    {
        Props.Remove(key);
    }

    #endregion

    public Node()
    {
        CachedType = GetType();
        Style = new NodeStyle(this);
        ClassList = new DOMTokenList(this, "class");
        Attributes = new NamedNodeMap(this);

        Style.FlexDirection = YGFlexDirection.Row;
        Style.FlexWrap = YGWrap.Wrap;
    }

    ~Node()
    {
        Dispose();
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;

        ChildFrameStyle.Dispose();
        ChildFrameColor.Dispose();
        Clear(); // remove children, but does not dispose them
        MeasureFuncHandle?.Free();
        YGNode->Dispose();
        base.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    public void DisposeRecursive()
    {
        if (IsDisposed)
            return;

        foreach (var child in this)
        {
            child.DisposeRecursive();
        }

        Dispose();
    }

    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public virtual void ApplyXmlNode(XmlNode xmlNode)
    {
        // handled in ParseXmlNode
        if (CachedType != typeof(Text))
        {
            // TODO: check type for ReadOnlySeString/string

            foreach (var field in CachedType.GetFields())
            {
                if (field.GetCustomAttribute<NodePropertyAttribute>() is NodePropertyAttribute propAttr && propAttr.UseChildrenInnerText)
                {
                    field.SetValue(this, xmlNode.InnerText.Trim());
                    break;
                }
            }

            foreach (var prop in CachedType.GetProperties())
            {
                if (prop.GetCustomAttribute<NodePropertyAttribute>() is NodePropertyAttribute propAttr && propAttr.UseChildrenInnerText)
                {
                    prop.SetValue(this, xmlNode.InnerText.Trim());
                    break;
                }
            }
        }

        ApplyXmlAttributes(xmlNode?.Attributes);
    }

    public void ApplyXmlAttributes(XmlAttributeCollection? attributes)
    {
        if (attributes == null)
            return;

        foreach (XmlAttribute attr in attributes)
        {
            ApplyXmlAttribute(attr.Name, attr.Value);
        }
    }

    public virtual void ApplyXmlAttribute(string name, string value)
    {
        foreach (var prop in CachedType.GetProperties())
        {
            if (prop.GetCustomAttribute<NodePropertyAttribute>() is NodePropertyAttribute propAttr) // TODO: cache?!
            {
                if ((propAttr.AttrName != null && propAttr.AttrName.Equals(name, StringComparison.InvariantCultureIgnoreCase)) || prop.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    prop.SetValue(this, prop.PropertyType.IsEnum
                        ? Enum.Parse(prop.PropertyType, value)
                        : Convert.ChangeType(value, prop.PropertyType));
                    return;
                }
            }
        }

        foreach (var field in CachedType.GetFields())
        {
            if (field.GetCustomAttribute<NodePropertyAttribute>() is NodePropertyAttribute propAttr) // TODO: cache?!
            {
                if ((propAttr.AttrName != null && propAttr.AttrName.Equals(name, StringComparison.InvariantCultureIgnoreCase)) || field.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    field.SetValue(this, field.FieldType.IsEnum
                        ? Enum.Parse(field.FieldType, value)
                        : Convert.ChangeType(value, field.FieldType));
                    return;
                }
            }
        }

        switch (name)
        {
            case "style":
                Style.Set(value);
                break;

            case "class":
                ClassName = value;
                break;

            default:
                GetDocument()?.Logger?.LogWarning("Unsupported attribute \"{attrName}\" with value \"{attrValue}\" on {typeName} {displayName}", name, value, TypeName, DisplayName);
                break;
        }
    }

    public virtual void Setup()
    {
    }

    public virtual void Update()
    {
        if (!IsSetupComplete)
        {
            Setup();
            IsSetupComplete = true;
        }

        UpdateChildNodes();
        UpdateRefs();
    }

    protected void UpdateRefs()
    {
        if (!IsDirty) return;

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

    public void Draw()
    {
        using var id = ImRaii.PushId(Guid.ToString());
        PreDraw();
        DrawNode();
        DrawChildNodes();
        PostDraw();
    }

    protected virtual void DrawNode()
    {
        // for nodes to implement
    }

    private void PreDraw()
    {
        ImGui.SetCursorPos(CumulativePosition);
        //ImGui.SetCursorPos(new Vector2(ComputedLeft, ComputedTop));

        DrawBackground();

        if (Style.Overflow is YGOverflow.Scroll or YGOverflow.Hidden)
        {
            ChildFrameStyle
                .Push(ImGuiStyleVar.FramePadding, Vector2.Zero)
                .Push(ImGuiStyleVar.WindowPadding, Vector2.Zero)
                .Push(ImGuiStyleVar.FrameBorderSize, 0)
                //.Push(ImGuiStyleVar.ChildRounding, ComputedStyle.BorderRadius)
                //.Push(ImGuiStyleVar.FrameRounding, ComputedStyle.BorderRadius)
                //.Push(ImGuiStyleVar.ScrollbarSize, 10)
                //.Push(ImGuiStyleVar.ScrollbarRounding, 0)
                .Push(ImGuiStyleVar.ChildBorderSize, 0);

            ChildFrameColor
                .Push(ImGuiCol.FrameBg, 0)
                /*
                .Push(ImGuiCol.ScrollbarBg, ComputedStyle.ScrollbarTrackColor.ToUInt())
                .Push(ImGuiCol.ScrollbarGrab, ComputedStyle.ScrollbarThumbColor.ToUInt())
                .Push(ImGuiCol.ScrollbarGrabActive, ComputedStyle.ScrollbarThumbActiveColor.ToUInt())
                .Push(ImGuiCol.ScrollbarGrabHovered, ComputedStyle.ScrollbarThumbHoverColor.ToUInt())*/;

            // HACK: abusing PaddingRight here for the scrollbar width. are there any better methods?
            Style.PaddingRight = Style.Overflow == YGOverflow.Scroll && HadOverflow
                ? ImGui.GetStyle().ScrollbarSize
                : 0;

            ImGui.BeginChildFrame(
                ImGui.GetID("##__ChildFrame"),
                ComputedSize,
                (Style.Overflow == YGOverflow.Scroll ? ImGuiWindowFlags.HorizontalScrollbar : ImGuiWindowFlags.None) | ImGuiWindowFlags.NoSavedSettings
            );
        }

        HandleMouse();
    }

    private void PostDraw()
    {
        if (Style.Overflow is YGOverflow.Scroll or YGOverflow.Hidden)
        {
            ImGui.EndChildFrame();

            ChildFrameStyle.Dispose();
            ChildFrameColor.Dispose();
        }

        DrawBorder();

        var paddingBottom = ComputedPaddingBottom;
        if (paddingBottom > 0)
            ImGui.Dummy(new Vector2(0, paddingBottom));
    }
}
