using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Text;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Gui.Enums;
using HaselCommon.Services;
using HaselCommon.Utils;
using ImGuiNET;

namespace HaselCommon.Gui;

public partial class Window
{
    private readonly Stopwatch _debugTimer = new();
    private double _debugLayoutTime;
    private double _debugUpdateTime;
    private double _debugDrawTime;
    private Node? _debugSelectedNode;

    private static bool DebugShowAllStyleProperties = true;
    private static readonly string[] DebugStyleValueNames = [
        "Display",
        "PositionType",
        "Direction",
        "Overflow",
        "FlexDirection",
        "JustifyContent",
        "AlignContent",
        "AlignItems",
        "AlignSelf",
        "FlexWrap",
        "Flex",
        "FlexGrow",
        "FlexShrink",
        "FlexBasis",
        "Margin",
        "MarginTop",
        "MarginBottom",
        "MarginLeft",
        "MarginRight",
        "MarginHorizontal",
        "MarginVertical",
        "MarginStart",
        "MarginEnd",
        "Position",
        "PositionTop",
        "PositionBottom",
        "PositionLeft",
        "PositionRight",
        "PositionHorizontal",
        "PositionVertical",
        "PositionStart",
        "PositionEnd",
        "Padding",
        "PaddingTop",
        "PaddingBottom",
        "PaddingLeft",
        "PaddingRight",
        "PaddingHorizontal",
        "PaddingVertical",
        "PaddingStart",
        "PaddingEnd",
        "Border",
        "BorderTop",
        "BorderBottom",
        "BorderLeft",
        "BorderRight",
        "BorderHorizontal",
        "BorderVertical",
        "BorderStart",
        "BorderEnd",
        "Gap",
        "RowGap",
        "ColumnGap",
        "Width",
        "Height",
        "MinWidth",
        "MinHeight",
        "MaxWidth",
        "MaxHeight",
        "AspectRatio",
    ];
    private static readonly string[] DebugLayoutNames = [
        "ConfigVersion",
        "GenerationCount",
        "Direction",
        "HadOverflow",
        "AbsoluteTop",
        "AbsoluteLeft",
        "Width",
        "Height",
        "MeasuredWidth",
        "MeasuredHeight",
        "PositionTop",
        "PositionBottom",
        "PositionLeft",
        "PositionRight",
        "MarginTop",
        "MarginBottom",
        "MarginLeft",
        "MarginRight",
        "BorderTop",
        "BorderBottom",
        "BorderLeft",
        "BorderRight",
        "PaddingTop",
        "PaddingBottom",
        "PaddingLeft",
        "PaddingRight",
    ];

    [Conditional("DEBUG")]
    private void DrawDebugWindow()
    {
        if (RootNode == null)
            return;

        if (!ImGui.Begin(WindowName + " | Debug", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            return;

        ImGui.TextUnformatted($"Layout: {_debugLayoutTime.ToString("0.000") + " ms"}");
        ImGuiUtils.VerticalSeparator();
        ImGui.SameLine();
        ImGui.TextUnformatted($"Update: {_debugUpdateTime.ToString("0.000") + " ms"}");
        ImGuiUtils.VerticalSeparator();
        ImGui.SameLine();
        ImGui.TextUnformatted($"Draw: {_debugDrawTime.ToString("0.000") + " ms"}");
        ImGui.Separator();

        using (var table = ImRaii.Table("TabBar", 2, ImGuiTableFlags.BordersInnerV, new Vector2(-1)))
        {
            if (table)
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 60);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 40);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                using (ImRaii.Child("NodeTree", new Vector2(-1)))
                    DrawNodeTree(RootNode);

                ImGui.TableNextColumn();

                DrawSelectedNode(_debugSelectedNode);
            }
        }

        ImGui.End();
    }

    private void DrawNodeTree(Node node)
    {
        var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow;

        if (_debugSelectedNode == node)
            flags |= ImGuiTreeNodeFlags.Selected;

        if (node.Count == 0)
            flags |= ImGuiTreeNodeFlags.Leaf;

        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        using var hiddenColor = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled), node.Display == Display.None);
        using var treeNode = ImRaii.TreeNode($"{node.DebugNodeOpenTag}###NodeOpen{node.Guid}", flags);

        if (ImGui.IsItemHovered())
            node._isDebugHovered = true;

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            _debugSelectedNode = node;

        using (ImRaii.PushColor(ImGuiCol.Text, textColor, node.Display == Display.None))
        {
            Service.Get<ImGuiContextMenuService>().Draw($"NodeOpenContextMenu{node.Guid}", (builder) =>
            {
                builder.Add(new ImGuiContextMenuEntry()
                {
                    Label = "Hide",
                    Visible = node.Display != Display.None,
                    ClickCallback = () => { node.Display = Display.None; }
                });

                builder.Add(new ImGuiContextMenuEntry()
                {
                    Label = "Show",
                    Visible = node.Display != Display.Flex,
                    ClickCallback = () => { node.Display = Display.Flex; }
                });
            });
        }

        if (!treeNode)
        {
            if (node.DebugHasClosingTag)
            {
                ImGui.SameLine(0, 0);
                ImGui.TextUnformatted($"...</{node.TagName}>");
            }
            return;
        }

        if (node.Count > 0)
        {
            foreach (var child in node)
            {
                DrawNodeTree(child);
            }
        }

        treeNode.Dispose();

        if (node.DebugHasClosingTag)
        {
            using (ImRaii.TreeNode($"</{node.TagName}>##NodeClose{node.Guid}", flags | ImGuiTreeNodeFlags.Leaf))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    _debugSelectedNode = node;
            }
        }
    }

    private static void DrawSelectedNode(Node? node)
    {
        if (node == null)
            return;

        using var tempNode = new Node() { Config = node.Config };

        using var tabs = ImRaii.TabBar("TabBar", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
        if (!tabs) return;

        using (var tab = ImRaii.TabItem("Node"))
        {
            if (tab)
            {
                using var table = ImRaii.Table("NodeTable", 2, ImGuiTableFlags.ScrollY);
                if (table)
                {
                    ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch, 40);
                    ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 60);

                    PrintRow("Type", node.GetType().FullName ?? node.GetType().Name);
                    PrintRow("Guid", node.Guid.ToString());

                    PrintRow("NodeType", node.NodeType);
                    PrintRow("AlwaysFormsContainingBlock", node.AlwaysFormsContainingBlock);
                    PrintRow("IsReferenceBaseline", node.IsReferenceBaseline);
                    PrintRow("HasNewLayout", node.HasNewLayout);
                    PrintRow("IsDirty", node.IsDirty);
                    PrintRow("HasBaselineFunc", node.HasBaselineFunc);
                    PrintRow("HasMeasureFunc", node.HasMeasureFunc);
                }
            }
        }

        using (var tab = ImRaii.TabItem("Style"))
        {
            if (tab)
            {
                var nodeType = node.GetType();

                if (ImGui.Button("Copy Style"))
                    CopyStyleToClipboard(node, tempNode);

                ImGui.SameLine();

                ImGui.Checkbox("Show all", ref DebugShowAllStyleProperties);

                using var _table = ImRaii.Table("StyleTable", 2, ImGuiTableFlags.ScrollY);
                if (_table)
                {
                    ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch, 40);
                    ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 60);

                    foreach (var propertyName in DebugStyleValueNames)
                    {
                        var propertyInfo = nodeType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                        if (propertyInfo != null)
                            PrintStyleRow(node, tempNode, propertyInfo);
                    }
                }
            }
        }

        using (var tab = ImRaii.TabItem("Layout"))
        {
            if (tab)
            {
                using var _table = ImRaii.Table("LayoutTable", 2, ImGuiTableFlags.ScrollY);
                if (_table)
                {
                    ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch, 40);
                    ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 60);

                    foreach (var propertyName in DebugLayoutNames)
                    {
                        var propertyInfo = typeof(LayoutResults).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                        if (propertyInfo != null)
                            PrintLayoutRow(node.Layout, propertyInfo);
                    }
                }
            }
        }
    }

    private static void CopyStyleToClipboard(Node node, Node tempNode)
    {
        var nodeType = node.GetType();
        var sb = new StringBuilder();

        foreach (var propertyName in DebugStyleValueNames)
        {
            var propertyInfo = nodeType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo == null) continue;

            var value = propertyInfo.GetValue(node);
            var defaultValue = propertyInfo.GetValue(tempNode);

            if (value == null || defaultValue == null)
                continue;

            if (propertyInfo.PropertyType.IsEnum)
            {
                if (value.Equals(defaultValue))
                    continue;

                sb.Append(propertyInfo.Name);
                sb.Append(" = ");
                sb.Append(propertyInfo.PropertyType.Name);
                sb.Append('.');
                sb.Append(value.ToString());
                sb.AppendLine(",");
            }
            else if (propertyInfo.PropertyType == typeof(StyleValue) && value is StyleValue styleValue && defaultValue is StyleValue defaultStyleValue)
            {
                if (styleValue == defaultStyleValue)
                    continue;

                sb.Append(propertyInfo.Name);
                sb.Append(" = ");
                switch (styleValue.Unit)
                {
                    case Unit.Auto:
                        sb.Append("StyleValue.Auto");
                        break;

                    case Unit.Undefined:
                        sb.Append("StyleValue.Undefined");
                        break;

                    case Unit.Percent:
                        sb.Append("StyleValue.Percent(");
                        sb.Append(styleValue.Value.ToString(CultureInfo.InvariantCulture));
                        if (styleValue.Value % 1 != 0)
                            sb.Append('f');
                        sb.Append(')');
                        break;

                    case Unit.Point:
                        sb.Append(styleValue.Value.ToString(CultureInfo.InvariantCulture));
                        if (styleValue.Value % 1 != 0)
                            sb.Append('f');
                        break;
                }
                sb.AppendLine(",");
            }
        }

        ImGui.SetClipboardText(sb.ToString());
    }

    private static void PrintStyleRow(Node node, Node tempNode, PropertyInfo propertyInfo)
    {
        var value = propertyInfo.GetValue(node);
        var defaultValue = propertyInfo.GetValue(tempNode);
        var isEqual = value?.Equals(defaultValue) ?? true;

        if (!DebugShowAllStyleProperties && isEqual)
            return;

        var defaultTextColor = ImGui.GetColorU32(ImGuiCol.Text);
        using var textColor = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled), isEqual);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(propertyInfo.Name);
        ImGui.TableNextColumn();

        if (propertyInfo.PropertyType.IsEnum)
        {
            ImGui.SetNextItemWidth(-1);
            using var combo = ImRaii.Combo($"###{propertyInfo.Name}_Combo", value?.ToString() ?? string.Empty);
            if (combo)
            {
                using var selectableColor = ImRaii.PushColor(ImGuiCol.Text, defaultTextColor);

                foreach (var val in propertyInfo.PropertyType.GetEnumValues())
                {
                    if (ImGui.Selectable(Enum.GetName(propertyInfo.PropertyType, val), val.Equals(value)))
                    {
                        propertyInfo.SetValue(node, val);
                    }
                }
            }
            return;
        }
        
        if (propertyInfo.PropertyType == typeof(StyleValue) && value is StyleValue styleValue)
        {
            const float UnitWidth = 100f;

            // special cases
            if (propertyInfo.Name is "FlexGrow" or "FlexShrink")
            {
                var intValue = (int)styleValue.Value;

                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputInt($"###{propertyInfo.Name}_Value", ref intValue))
                {
                    if (intValue < 0) intValue = 0;
                    propertyInfo.SetValue(node, styleValue with { Value = intValue });
                }

                return;
            }

            if (styleValue.Unit is not Unit.Undefined and not Unit.Auto)
            {
                var floatValue = styleValue.Value;

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - UnitWidth - ImGui.GetStyle().ItemInnerSpacing.X);
                if (ImGui.InputFloat($"###{propertyInfo.Name}_Value", ref floatValue, 1, 10))
                {
                    propertyInfo.SetValue(node, styleValue with { Value = floatValue });
                }

                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
            }

            ImGui.SetNextItemWidth(styleValue.Unit is Unit.Undefined or Unit.Auto ? -1 : UnitWidth);
            using var combo = ImRaii.Combo($"###{propertyInfo.Name}_Unit", $"{styleValue.Unit}");
            if (!combo) return;

            using var selectableColor = ImRaii.PushColor(ImGuiCol.Text, defaultTextColor);

            foreach (var val in Enum.GetValues<Unit>())
            {
                if (ImGui.Selectable(Enum.GetName(val), val.Equals(value)))
                {
                    if (float.IsNaN(styleValue.Value))
                        propertyInfo.SetValue(node, styleValue with { Value = 0, Unit = val });
                    else
                        propertyInfo.SetValue(node, styleValue with { Unit = val });
                }
            }
        }
    }

    private static void PrintLayoutRow(LayoutResults layoutResults, PropertyInfo propertyInfo)
    {
        var value = propertyInfo.GetValue(layoutResults);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(propertyInfo.Name);
        ImGui.TableNextColumn();

        if (value is float floatVal)
            ImGui.TextUnformatted($"{floatVal.ToString("0", CultureInfo.InvariantCulture)}");
        else if (value is StyleValue styleValue)
            ImGui.TextUnformatted($"{styleValue}");
        else
            ImGui.TextUnformatted(value?.ToString() ?? "null");

    }

    private static void PrintRow(string label, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(value);
    }

    private static void PrintRow<T>(string label, T value) where T : struct, Enum
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }

    private static void PrintRow(string label, bool value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }
}
