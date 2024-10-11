using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Extensions;
using HaselCommon.Graphics;
using HaselCommon.Gui.Yoga.Attributes;
using HaselCommon.Services;
using ImGuiNET;
using Lumina.Text.ReadOnly;
using YogaSharp;

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

    private static string EnumQueryText = string.Empty;

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

                DrawNodeProperties(_debugSelectedNode);
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
        using var hiddenColor = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled), node.Display == YGDisplay.None);
        using var treeNode = ImRaii.TreeNode($"{node.DebugNodeOpenTag}###NodeOpen{node.Guid}", flags);

        if (ImGui.IsItemHovered())
            node._isDebugHovered = true;

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            _debugSelectedNode = node;

        using (ImRaii.PushColor(ImGuiCol.Text, textColor, node.Display == YGDisplay.None))
        {
            Service.Get<ImGuiContextMenuService>().Draw($"NodeOpenContextMenu{node.Guid}", (builder) =>
            {
                builder.Add(new ImGuiContextMenuEntry()
                {
                    Label = "Hide",
                    Visible = node.Display != YGDisplay.None,
                    ClickCallback = () => { node.Display = YGDisplay.None; }
                });

                builder.Add(new ImGuiContextMenuEntry()
                {
                    Label = "Show",
                    Visible = node.Display != YGDisplay.Flex,
                    ClickCallback = () => { node.Display = YGDisplay.Flex; }
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

    private static void DrawNodeProperties(Node? node)
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

        var tabIndex = -1;
        foreach (var category in categories)
        {
            tabIndex++;

            using var tab = ImRaii.TabItem($"{category}###NodeTab{tabIndex}");
            if (!tab) continue;

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
                    PrintEditableRow(node, category, propInfo);
                else
                    PrintReadOnlyRow(node, category, propInfo);
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

            if (propInfo.PropertyType.IsEnum)
            {
                sb.Append(propInfo.Name);
                sb.Append(" = ");
                sb.Append(propInfo.PropertyType.Name);
                sb.Append('.');
                sb.Append(value.ToString());
                sb.AppendLine(",");
            }
            else if (propInfo.PropertyType == typeof(YGValue) && value is YGValue styleLength)
            {
                sb.Append(propInfo.Name);
                sb.Append(" = ");
                switch (styleLength.Unit)
                {
                    case YGUnit.Auto:
                        sb.Append("StyleLength.Auto");
                        break;

                    case YGUnit.Undefined:
                        sb.Append("StyleLength.Undefined");
                        break;

                    case YGUnit.Percent:
                        sb.Append("StyleLength.Percent(");
                        sb.Append(styleLength.Value.ToString(CultureInfo.InvariantCulture));
                        if (styleLength.Value % 1 != 0)
                            sb.Append('f');
                        sb.Append(')');
                        break;

                    case YGUnit.Point:
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

    private static void PrintEditableRow(Node node, string category, PropertyInfo propertyInfo)
    {
        var value = propertyInfo.GetValue(node);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(propertyInfo.Name);
        ImGui.TableNextColumn();

        var propertyType = propertyInfo.PropertyType;
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            propertyType = propertyType.GetGenericArguments()[0];

        if (value is null)
        {
            if (ImGui.Button($"Create###{propertyInfo.Name}_NullableCreate"))
            {
                propertyInfo.SetValue(node, Activator.CreateInstance(propertyType));
            }
            return;
        }

        if (propertyInfo.PropertyType.IsEnum)
        {
            ImGui.SetNextItemWidth(-1);
            using var combo = ImRaii.Combo($"###{propertyInfo.Name}_Combo", value?.ToString() ?? string.Empty);
            if (combo)
            {
                ImGui.InputTextWithHint($"###{propertyInfo.Name}_ComboSearch", "Search...", ref EnumQueryText, 255);

                foreach (var val in propertyInfo.PropertyType.GetEnumValues())
                {
                    var name = Enum.GetName(propertyInfo.PropertyType, val) ?? string.Empty;

                    if (!string.IsNullOrEmpty(EnumQueryText) && !name.Contains(EnumQueryText, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (ImGui.Selectable(name, val.Equals(value)))
                    {
                        propertyInfo.SetValue(node, val);
                        EnumQueryText = string.Empty;
                    }
                }
            }
            return;
        }

        if (propertyInfo.PropertyType == typeof(YGValue) && value is YGValue styleLength)
        {
            const float UnitWidth = 100f;

            // special cases
            if (propertyInfo.Name is "FlexGrow" or "FlexShrink")
            {
                var intValue = (int)styleLength.Value;

                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputInt($"###{node.Guid}_{propertyInfo.Name}_Value", ref intValue))
                {
                    if (intValue < 0) intValue = 0;
                    propertyInfo.SetValue(node, styleLength with { Value = intValue });
                }

                return;
            }

            if (styleLength.Unit is not YGUnit.Undefined and not YGUnit.Auto)
            {
                var floatValue = styleLength.Value;

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - UnitWidth - ImGui.GetStyle().ItemInnerSpacing.X);
                if (ImGui.InputFloat($"###{node.Guid}_{propertyInfo.Name}_Value", ref floatValue, 1, 10))
                {
                    propertyInfo.SetValue(node, styleLength with { Value = floatValue });
                }

                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
            }

            ImGui.SetNextItemWidth(styleLength.Unit is YGUnit.Undefined or YGUnit.Auto ? -1 : UnitWidth);
            using var combo = ImRaii.Combo($"###{node.Guid}_{propertyInfo.Name}_Unit", $"{styleLength.Unit}");
            if (!combo) return;

            foreach (var val in Enum.GetValues<YGUnit>())
            {
                if (ImGui.Selectable(Enum.GetName(val), val.Equals(value)))
                {
                    if (float.IsNaN(styleLength.Value))
                        propertyInfo.SetValue(node, styleLength with { Value = 0, Unit = val });
                    else
                        propertyInfo.SetValue(node, styleLength with { Unit = val });
                }
            }
            return;
        }

        if (propertyType == typeof(Color) && value is Color color)
        {
            var vecCol = (Vector4)color;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.ColorEdit4($"###{node.Guid}_{propertyInfo.Name}", ref vecCol))
                propertyInfo.SetValue(node, new Color(vecCol));
            return;
        }

        if (propertyInfo.PropertyType == typeof(uint) && value is uint uintVal)
        {
            var intVal = (int)uintVal;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputInt($"###{node.Guid}_{propertyInfo.Name}", ref intVal))
                propertyInfo.SetValue(node, (uint)intVal);
            return;
        }

        if (propertyInfo.PropertyType == typeof(float) && value is float floatVal)
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat($"###{node.Guid}_{propertyInfo.Name}", ref floatVal, 1, 10))
                propertyInfo.SetValue(node, floatVal);
            return;
        }

        if (propertyInfo.PropertyType == typeof(ReadOnlySeString) && value is ReadOnlySeString rossVal)
        {
            var macroString = rossVal.ToString();
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputText($"###{node.Guid}_{propertyInfo.Name}", ref macroString, 1024))
                propertyInfo.SetValue(node, ReadOnlySeString.FromMacroString(macroString));
            return;
        }

        ImGui.TextUnformatted($"Unsupported Type: {propertyInfo.PropertyType.Name}");
    }

    private static void PrintReadOnlyRow(Node node, string category, PropertyInfo propertyInfo)
    {
        var value = propertyInfo.GetValue(node);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(propertyInfo.Name);
        ImGui.TableNextColumn();

        if (value is float floatVal)
        {
            ImGui.TextUnformatted($"{floatVal.ToString(CultureInfo.InvariantCulture)}");
        }
        else if (value is YGValue styleLength)
        {
            ImGui.TextUnformatted($"{styleLength}");
        }
        else if (value is Color color)
        {
            var vecCol = (Vector4)(color with { R = color.B, B = color.R });
            ImGui.SetNextItemWidth(-1);
            ImGui.ColorEdit4($"###{node.Guid}_{propertyInfo.Name}_ColorEdit", ref vecCol);
        }
        else
        {
            ImGui.TextUnformatted(value?.ToString() ?? "null");
        }
    }
}
