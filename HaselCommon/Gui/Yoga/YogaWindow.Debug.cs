using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Extensions;
using HaselCommon.Gui.Yoga.Attributes;
using HaselCommon.Gui.Yoga.Enums;
using HaselCommon.Services;
using ImGuiNET;

namespace HaselCommon.Gui.Yoga;

public partial class YogaWindow
{
    private readonly Stopwatch _debugTimer = new();
    private double _debugLayoutTime;
    private double _debugUpdateTime;
    private double _debugDrawTime;
    private Node? _debugSelectedNode;

    private static readonly string[] DebugCategories = ["Node", "Style", "Layout"];
    private static readonly string[] DebugSortedNodePropNames = [
        // Node
        "TagName",
        "Guid",
        "NodeType",
        "AlwaysFormsContainingBlock",
        "IsReferenceBaseline",
        "HasNewLayout",
        "IsDirty",
        "HasBaselineFunc",
        "HasMeasureFunc",

        // Style
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
        "MarginAll",
        "MarginTop",
        "MarginBottom",
        "MarginLeft",
        "MarginRight",
        "MarginHorizontal",
        "MarginVertical",
        "MarginStart",
        "MarginEnd",
        "PositionAll",
        "PositionTop",
        "PositionBottom",
        "PositionLeft",
        "PositionRight",
        "PositionHorizontal",
        "PositionVertical",
        "PositionStart",
        "PositionEnd",
        "PaddingAll",
        "PaddingTop",
        "PaddingBottom",
        "PaddingLeft",
        "PaddingRight",
        "PaddingHorizontal",
        "PaddingVertical",
        "PaddingStart",
        "PaddingEnd",
        "BorderAll",
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

        // Layout
        "Direction",
        "HadOverflow",
        "ComputedWidth",
        "ComputedHeight",
        "ComputedTop",
        "ComputedBottom",
        "ComputedLeft",
        "ComputedRight",
        "ComputedMarginTop",
        "ComputedMarginBottom",
        "ComputedMarginLeft",
        "ComputedMarginRight",
        "ComputedBorderTop",
        "ComputedBorderBottom",
        "ComputedBorderLeft",
        "ComputedBorderRight",
        "ComputedPaddingTop",
        "ComputedPaddingBottom",
        "ComputedPaddingLeft",
        "ComputedPaddingRight",
    ];

    private static bool DebugShowAllStyleProperties;

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

        var nodeType = node.GetType();

        using var tabs = ImRaii.TabBar("TabBar", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
        if (!tabs) return;

        var categories = DebugCategories.Concat(
            nodeType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(pi => pi.GetCustomAttribute<NodeProp>()?.Category)
                .Where(category => !string.IsNullOrEmpty(category))
                .Cast<string>())
            .Distinct();

        foreach (var category in categories)
        {
            using var tab = ImRaii.TabItem(category);
            if (!tab) continue;

            if (category == "Style")
            {
                if (ImGui.Button("Copy Style"))
                    CopyStyleToClipboard(node);

                ImGui.SameLine();

                ImGui.Checkbox("Show all", ref DebugShowAllStyleProperties);
            }

            using var table = ImRaii.Table("DataTable", 2, ImGuiTableFlags.ScrollY);
            if (!table) continue;

            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch, 40);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 60);

            var props = nodeType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(pi =>
                {
                    var index = DebugSortedNodePropNames.IndexOf(pi.Name);
                    return index == -1 ? int.MaxValue : index;
                })
                .ThenBy(pi => pi.Name);

            foreach (var propInfo in props)
            {
                if (propInfo.GetCustomAttribute<NodeProp>() is not NodeProp nodePropAttr)
                    continue;

                if (nodePropAttr.Category != category)
                    continue;

                if (nodePropAttr.Editable)
                    PrintEditableRow(node, propInfo);
                else
                    PrintReadOnlyRow(node, propInfo);
            }
        }
    }

    private static void CopyStyleToClipboard(Node node)
    {
        var nodeType = node.GetType();
        var sb = new StringBuilder();

        foreach (var propInfo in nodeType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (propInfo.GetCustomAttribute<NodeProp>() is not NodeProp nodePropAttr)
                continue;

            if (nodePropAttr.Category != "Style")
                continue;

            var value = propInfo.GetValue(node);
            if (value == null)
                continue;

            if (!node._changedProps.Contains(propInfo.Name))
                continue;

            if (propInfo.PropertyType.IsEnum)
            {
                sb.Append(propInfo.Name);
                sb.Append(" = ");
                sb.Append(propInfo.PropertyType.Name);
                sb.Append('.');
                sb.Append(value.ToString());
                sb.AppendLine(",");
            }
            else if (propInfo.PropertyType == typeof(StyleLength) && value is StyleLength styleLength)
            {
                sb.Append(propInfo.Name);
                sb.Append(" = ");
                switch (styleLength.Unit)
                {
                    case Unit.Auto:
                        sb.Append("StyleLength.Auto");
                        break;

                    case Unit.Undefined:
                        sb.Append("StyleLength.Undefined");
                        break;

                    case Unit.Percent:
                        sb.Append("StyleLength.Percent(");
                        sb.Append(styleLength.Value.ToString(CultureInfo.InvariantCulture));
                        if (styleLength.Value % 1 != 0)
                            sb.Append('f');
                        sb.Append(')');
                        break;

                    case Unit.Point:
                        sb.Append(styleLength.Value.ToString(CultureInfo.InvariantCulture));
                        if (styleLength.Value % 1 != 0)
                            sb.Append('f');
                        break;
                }
                sb.AppendLine(",");
            }
        }

        ImGui.SetClipboardText(sb.ToString());
    }

    private static void PrintEditableRow(Node node, PropertyInfo propertyInfo)
    {
        var value = propertyInfo.GetValue(node);
        var wasChanged = node._changedProps.Contains(propertyInfo.Name);

        if (!DebugShowAllStyleProperties && !wasChanged)
            return;

        var defaultTextColor = ImGui.GetColorU32(ImGuiCol.Text);
        using var textColor = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled), !wasChanged);

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

        if (propertyInfo.PropertyType == typeof(StyleLength) && value is StyleLength styleLength)
        {
            const float UnitWidth = 100f;

            // special cases
            if (propertyInfo.Name is "FlexGrow" or "FlexShrink")
            {
                var intValue = (int)styleLength.Value;

                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputInt($"###{propertyInfo.Name}_Value", ref intValue))
                {
                    if (intValue < 0) intValue = 0;
                    propertyInfo.SetValue(node, styleLength with { Value = intValue });
                }

                return;
            }

            if (styleLength.Unit is not Unit.Undefined and not Unit.Auto)
            {
                var floatValue = styleLength.Value;

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - UnitWidth - ImGui.GetStyle().ItemInnerSpacing.X);
                if (ImGui.InputFloat($"###{propertyInfo.Name}_Value", ref floatValue, 1, 10))
                {
                    propertyInfo.SetValue(node, styleLength with { Value = floatValue });
                }

                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
            }

            ImGui.SetNextItemWidth(styleLength.Unit is Unit.Undefined or Unit.Auto ? -1 : UnitWidth);
            using var combo = ImRaii.Combo($"###{propertyInfo.Name}_Unit", $"{styleLength.Unit}");
            if (!combo) return;

            using var selectableColor = ImRaii.PushColor(ImGuiCol.Text, defaultTextColor);

            foreach (var val in Enum.GetValues<Unit>())
            {
                if (ImGui.Selectable(Enum.GetName(val), val.Equals(value)))
                {
                    if (float.IsNaN(styleLength.Value))
                        propertyInfo.SetValue(node, styleLength with { Value = 0, Unit = val });
                    else
                        propertyInfo.SetValue(node, styleLength with { Unit = val });
                }
            }
        }
    }

    private static void PrintReadOnlyRow(Node node, PropertyInfo propertyInfo)
    {
        var value = propertyInfo.GetValue(node);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(propertyInfo.Name);
        ImGui.TableNextColumn();

        if (value is float floatVal)
            ImGui.TextUnformatted($"{floatVal.ToString(CultureInfo.InvariantCulture)}");
        else if (value is StyleLength styleLength)
            ImGui.TextUnformatted($"{styleLength}");
        else
            ImGui.TextUnformatted(value?.ToString() ?? "null");
    }
}
