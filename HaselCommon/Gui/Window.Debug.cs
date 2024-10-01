using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Gui.Enums;
using HaselCommon.Utils;
using ImGuiNET;

namespace HaselCommon.Gui;

public partial class Window
{
    private readonly Stopwatch _debugTimer = new();
    private double _debugLayoutTime;
    private double _debugUpdateTime;
    private double _debugDrawTime;

    public Node? SelectedDebugNode { get; private set; }

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
        // ImGuiUtils.VerticalSeparator();
        // ImGui.SameLine();
        // if (ImGui.Button("Set Style Dirty"))
        //     RootNode.SetStyleDirty();
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

                DrawSelectedNode(SelectedDebugNode);
            }
        }

        ImGui.End();
    }

    private void DrawNodeTree(Node node)
    {
        var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow;

        if (SelectedDebugNode == node)
            flags |= ImGuiTreeNodeFlags.Selected;

        if (node.Count == 0)
            flags |= ImGuiTreeNodeFlags.Leaf;

        using var treeNode = ImRaii.TreeNode($"{node.DebugNodeOpenTag}##NodeOpen{node.Guid}", flags);

        if (ImGui.IsItemHovered())
            node._isDebugHovered = true;

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            SelectedDebugNode = node;

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
                    SelectedDebugNode = node;
            }
        }
    }

    private static void DrawSelectedNode(Node? node)
    {
        if (node == null)
            return;

        using var tabs = ImRaii.TabBar("TabBar", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
        if (!tabs) return;

        using (var tab = ImRaii.TabItem("Node"))
        {
            if (tab)
            {
                using (var table = ImRaii.Table("NodeTable", 2))
                {
                    if (table)
                    {
                        ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch, 40);
                        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 60);

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

                /*
                if (node.Attributes.Count > 0)
                {
                    ImGui.Separator();
                    ImGui.TextUnformatted("Attributes:");

                    using (var table = ImRaii.Table("NodeAttributeTable", 2))
                    {
                        if (table)
                        {
                            ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.WidthStretch, 40);
                            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 60);

                            foreach (var attr in node.Attributes)
                            {
                                PrintRow(attr.Key, attr.Value ?? string.Empty);
                            }

                            if (node is Text textNode)
                            {
                                PrintRow("Text", textNode.TextValue);
                            }
                        }
                    }
                }
                */
            }
        }

        using (var tab = ImRaii.TabItem("Style"))
        {
            if (tab)
            {
                using var _table = ImRaii.Table("StyleTable", 2, ImGuiTableFlags.ScrollY);
                if (_table)
                {
                    ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch, 40);
                    ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 60);

                    PrintRow("Display", node.Display, (value) => node.Display = value);
                    PrintRow("PositionType", node.PositionType, (value) => node.PositionType = value);
                    PrintRow("Direction", node.Direction, (value) => node.Direction = value);
                    PrintRow("Overflow", node.Overflow, (value) => node.Overflow = value);
                    PrintRow("FlexDirection", node.FlexDirection, (value) => node.FlexDirection = value);
                    PrintRow("JustifyContent", node.JustifyContent, (value) => node.JustifyContent = value);
                    PrintRow("AlignContent", node.AlignContent, (value) => node.AlignContent = value);
                    PrintRow("AlignItems", node.AlignItems, (value) => node.AlignItems = value);
                    PrintRow("AlignSelf", node.AlignSelf, (value) => node.AlignSelf = value);
                    PrintRow("FlexWrap", node.FlexWrap, (value) => node.FlexWrap = value);
                    PrintRow("Flex", node.Flex, (value) => node.Flex = value);
                    PrintRow("FlexGrow", node.FlexGrow, (value) => node.FlexGrow = value);
                    PrintRow("FlexShrink", node.FlexShrink, (value) => node.FlexShrink = value);
                    PrintRow("FlexBasis", node.FlexBasis, (value) => node.FlexBasis = value);

                    PrintRow("Margin", node.Margin, (value) => node.Margin = value);
                    PrintRow("MarginTop", node.MarginTop, (value) => node.MarginTop = value);
                    PrintRow("MarginBottom", node.MarginBottom, (value) => node.MarginBottom = value);
                    PrintRow("MarginLeft", node.MarginLeft, (value) => node.MarginLeft = value);
                    PrintRow("MarginRight", node.MarginRight, (value) => node.MarginRight = value);
                    PrintRow("MarginHorizontal", node.MarginHorizontal, (value) => node.MarginHorizontal = value);
                    PrintRow("MarginVertical", node.MarginVertical, (value) => node.MarginVertical = value);
                    PrintRow("MarginStart", node.MarginStart, (value) => node.MarginStart = value);
                    PrintRow("MarginEnd", node.MarginEnd, (value) => node.MarginEnd = value);

                    PrintRow("Position", node.Position, (value) => node.Position = value);
                    PrintRow("PositionTop", node.PositionTop, (value) => node.PositionTop = value);
                    PrintRow("PositionBottom", node.PositionBottom, (value) => node.PositionBottom = value);
                    PrintRow("PositionLeft", node.PositionLeft, (value) => node.PositionLeft = value);
                    PrintRow("PositionRight", node.PositionRight, (value) => node.PositionRight = value);
                    PrintRow("PositionHorizontal", node.PositionHorizontal, (value) => node.PositionHorizontal = value);
                    PrintRow("PositionVertical", node.PositionVertical, (value) => node.PositionVertical = value);
                    PrintRow("PositionStart", node.PositionStart, (value) => node.PositionStart = value);
                    PrintRow("PositionEnd", node.PositionEnd, (value) => node.PositionEnd = value);

                    PrintRow("Padding", node.Padding, (value) => node.Padding = value);
                    PrintRow("PaddingTop", node.PaddingTop, (value) => node.PaddingTop = value);
                    PrintRow("PaddingBottom", node.PaddingBottom, (value) => node.PaddingBottom = value);
                    PrintRow("PaddingLeft", node.PaddingLeft, (value) => node.PaddingLeft = value);
                    PrintRow("PaddingRight", node.PaddingRight, (value) => node.PaddingRight = value);
                    PrintRow("PaddingHorizontal", node.PaddingHorizontal, (value) => node.PaddingHorizontal = value);
                    PrintRow("PaddingVertical", node.PaddingVertical, (value) => node.PaddingVertical = value);
                    PrintRow("PaddingStart", node.PaddingStart, (value) => node.PaddingStart = value);
                    PrintRow("PaddingEnd", node.PaddingEnd, (value) => node.PaddingEnd = value);

                    PrintRow("Border", node.Border, (value) => node.Border = value);
                    PrintRow("BorderTop", node.BorderTop, (value) => node.BorderTop = value);
                    PrintRow("BorderBottom", node.BorderBottom, (value) => node.BorderBottom = value);
                    PrintRow("BorderLeft", node.BorderLeft, (value) => node.BorderLeft = value);
                    PrintRow("BorderRight", node.BorderRight, (value) => node.BorderRight = value);
                    PrintRow("BorderHorizontal", node.BorderHorizontal, (value) => node.BorderHorizontal = value);
                    PrintRow("BorderVertical", node.BorderVertical, (value) => node.BorderVertical = value);
                    PrintRow("BorderStart", node.BorderStart, (value) => node.BorderStart = value);
                    PrintRow("BorderEnd", node.BorderEnd, (value) => node.BorderEnd = value);

                    PrintRow("Gap", node.Gap, (value) => node.Gap = value);
                    PrintRow("RowGap", node.RowGap, (value) => node.RowGap = value);
                    PrintRow("ColumnGap", node.ColumnGap, (value) => node.ColumnGap = value);

                    PrintRow("Width", node.Width, (value) => node.Width = value);
                    PrintRow("Height", node.Height, (value) => node.Height = value);

                    PrintRow("MinWidth", node.MinWidth, (value) => node.MinWidth = value);
                    PrintRow("MinHeight", node.MinHeight, (value) => node.MinHeight = value);

                    PrintRow("MaxWidth", node.MaxWidth, (value) => node.MaxWidth = value);
                    PrintRow("MaxHeight", node.MaxHeight, (value) => node.MaxHeight = value);

                    PrintRow("AspectRatio", node.AspectRatio, (value) => node.AspectRatio = value);
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

                    PrintRow("ConfigVersion", node.Layout.ConfigVersion);
                    PrintRow("GenerationCount", node.Layout.GenerationCount);

                    PrintRow("Direction", node.Layout.Direction);
                    PrintRow("HadOverflow", node.Layout.HadOverflow);

                    // Absolute Position
                    PrintRow("AbsoluteTop", node.AbsolutePosition.Y);
                    PrintRow("AbsoluteLeft", node.AbsolutePosition.X);

                    // Dimensions
                    PrintRow("Width", node.Layout.Width);
                    PrintRow("Height", node.Layout.Height);

                    // MeasuredDimensions
                    if (node.HasMeasureFunc)
                    {
                        PrintRow("MeasuredWidth", node.Layout.MeasuredWidth);
                        PrintRow("MeasuredHeight", node.Layout.MeasuredHeight);
                    }

                    // Position
                    PrintRow("PositionTop", node.Layout.PositionTop);
                    PrintRow("PositionBottom", node.Layout.PositionBottom);
                    PrintRow("PositionLeft", node.Layout.PositionLeft);
                    PrintRow("PositionRight", node.Layout.PositionRight);

                    // Margin
                    PrintRow("MarginTop", node.Layout.MarginTop);
                    PrintRow("MarginBottom", node.Layout.MarginBottom);
                    PrintRow("MarginLeft", node.Layout.MarginLeft);
                    PrintRow("MarginRight", node.Layout.MarginRight);

                    // Border
                    PrintRow("BorderTop", node.Layout.BorderTop);
                    PrintRow("BorderBottom", node.Layout.BorderBottom);
                    PrintRow("BorderLeft", node.Layout.BorderLeft);
                    PrintRow("BorderRight", node.Layout.BorderRight);

                    // Padding
                    PrintRow("PaddingTop", node.Layout.PaddingTop);
                    PrintRow("PaddingBottom", node.Layout.PaddingBottom);
                    PrintRow("PaddingLeft", node.Layout.PaddingLeft);
                    PrintRow("PaddingRight", node.Layout.PaddingRight);
                }
            }
        }
    }

    private static void PrintRow(string label, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(value);
        //ImGui.InputText($"##{text}", ref value, (uint)value.Length, ImGuiInputTextFlags.ReadOnly);
    }

    private static void PrintRow<T>(string label, T value) where T : struct, Enum
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }

    private static void PrintRow<T>(string label, T value, Action<T> setter) where T : struct, Enum
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();

        using var combo = ImRaii.Combo($"##{label}_Combo", $"{value}");
        if (!combo) return;

        foreach (var val in Enum.GetValues<T>())
        {
            if (ImGui.Selectable(Enum.GetName(val), val.Equals(value)))
            {
                setter(val);
            }
        }
    }

    private static void PrintRow(string label, bool value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }

    private static void PrintRow(string label, float value)
    {
        if (float.IsNaN(value))
            return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value.ToString("0.###", CultureInfo.InvariantCulture)}");
    }

    private static void PrintRow(string label, StyleValue value, Action<StyleValue> setter)
    {
        const float UnitWidth = 100f;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();

        // special cases
        if (label is "FlexGrow" or "FlexShrink")
        {
            var intValue = (int)value.Value;
            if (ImGui.InputInt($"##{label}_Value", ref intValue))
            {
                if (intValue < 0) intValue = 0;
                setter(value with { Value = intValue });
            }

            return;
        }

        if (value.Unit is not Unit.Undefined and not Unit.Auto)
        {
            var floatValue = value.Value;

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - UnitWidth - ImGui.GetStyle().ItemInnerSpacing.X);
            if (ImGui.InputFloat($"##{label}_Value", ref floatValue, 1, 10))
            {
                setter(value with { Value = floatValue });
            }

            ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        }

        ImGui.SetNextItemWidth(UnitWidth);
        using var combo = ImRaii.Combo($"##{label}_Unit", $"{value.Unit}");
        if (!combo) return;

        foreach (var val in Enum.GetValues<Unit>())
        {
            if (ImGui.Selectable(Enum.GetName(val), val.Equals(value)))
            {
                if (float.IsNaN(value.Value))
                    setter(value with { Value = 0, Unit = val });
                else
                    setter(value with { Unit = val });
            }
        }
    }
}
