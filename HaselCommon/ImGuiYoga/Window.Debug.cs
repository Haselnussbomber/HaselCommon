using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Common.Lua;
using HaselCommon.ImGuiYoga.Attributes;
using HaselCommon.Utils;
using ImGuiNET;
using YogaSharp;
using static System.Net.Mime.MediaTypeNames;

namespace HaselCommon.ImGuiYoga;

public partial class Window
{
    private readonly Stopwatch DebugTimer = new();
    private double DebugLayoutTime;
    private double DebugDrawTime;

    public Node? SelectedDebugNode { get; private set; }

    [Conditional("DEBUG")]
    private void DrawDebugWindow()
    {
        if (Document == null)
            return;

        if (!ImGui.Begin(WindowName + " | Debug", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            return;

        ImGui.TextUnformatted($"LayoutTime: {DebugLayoutTime.ToString("0.000") + " ms"}");
        ImGuiUtils.VerticalSeparator();
        ImGui.SameLine();
        ImGui.TextUnformatted($"DrawTime: {DebugDrawTime.ToString("0.000") + " ms"}");
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
                    DrawNodeTree(Document);

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

        using var treeNode = ImRaii.TreeNode($"{node.AsHtmlOpenTag}##NodeOpen{node.Guid}", flags);

        if (ImGui.IsItemHovered())
            node.IsDebugHovered = true;

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            SelectedDebugNode = node;

        if (!treeNode)
        {
            ImGui.SameLine(0, 0);
            ImGui.TextUnformatted($"...</{node.TagName}>");
            return;
        }

        if (node.Count > 0)
        {
            foreach (var child in node)
            {
                DrawNodeTree(child);
            }

            treeNode.Dispose();
            using (ImRaii.TreeNode($"</{node.TagName}>##NodeClose{node.Guid}", flags | ImGuiTreeNodeFlags.Leaf))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    SelectedDebugNode = node;
            }
        }
    }

    private void DrawSelectedNode(Node? node)
    {
        if (node == null)
            return;

        using var id = ImRaii.PushId($"NodeInfo_{node.Guid}");

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
                        ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.WidthStretch, 33);
                        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 67);

                        PrintRow("Type", node.TypeName);
                        PrintRow("Guid", node.Guid.ToString());
                        PrintRow("IsDirty", node.IsDirty);
                        PrintRow("HasNewLayout", node.HasNewLayout);
                    }
                }

                if (node.Attributes.Count > 0)
                {
                    ImGui.Separator();
                    ImGui.TextUnformatted("Attributes:");

                    using (var table = ImRaii.Table("NodeAttributeTable", 2))
                    {
                        if (table)
                        {
                            ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.WidthStretch, 33);
                            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 67);

                            foreach (var attr in node.Attributes)
                            {
                                PrintRow(attr.Key, attr.Value);
                            }

                            if (node is Text textNode)
                            {
                                PrintRow("Text", textNode.TextValue);
                            }
                        }
                    }
                }
            }
        }

        using (var tab = ImRaii.TabItem("Style"))
        {
            if (tab)
            {
                using var _table = ImRaii.Table("StyleTable", 2);
                if (_table)
                {
                    if (node.Style.AlignContent != YGAlign.FlexStart) PrintRow("AlignContent", node.Style.AlignContent);
                    if (node.Style.AlignItems != YGAlign.Stretch) PrintRow("AlignItems", node.Style.AlignItems);
                    if (node.Style.AlignSelf != YGAlign.Auto) PrintRow("AlignSelf", node.Style.AlignSelf);
                    PrintRow("AspectRatio", node.Style.AspectRatio);
                    PrintRow("Border", node.Style.Border);
                    PrintRow("BorderTop", node.Style.BorderTop);
                    PrintRow("BorderRight", node.Style.BorderRight);
                    PrintRow("BorderBottom", node.Style.BorderBottom);
                    PrintRow("BorderLeft", node.Style.BorderLeft);
                    PrintRow("BorderHorizontal", node.Style.BorderHorizontal);
                    PrintRow("BorderVertical", node.Style.BorderVertical);
                    PrintRow("BorderRadius", node.Style.BorderRadius);
                    PrintRow("BorderRadiusTopLeft", node.Style.BorderRadiusTopLeft);
                    PrintRow("BorderRadiusTopRight", node.Style.BorderRadiusTopRight);
                    PrintRow("BorderRadiusBottomLeft", node.Style.BorderRadiusBottomLeft);
                    PrintRow("BorderRadiusBottomRight", node.Style.BorderRadiusBottomRight);
                    PrintRow("BorderRadiusTop", node.Style.BorderRadiusTop);
                    PrintRow("BorderRadiusBottom", node.Style.BorderRadiusBottom);
                    PrintRowColor("BorderColor", node.Style.BorderColor);
                    PrintRowColor("BorderColorTop", node.Style.BorderColorTop);
                    PrintRowColor("BorderColorRight", node.Style.BorderColorRight);
                    PrintRowColor("BorderColorBottom", node.Style.BorderColorBottom);
                    PrintRowColor("BorderColorLeft", node.Style.BorderColorLeft);
                    PrintRowColor("BorderColorHorizontal", node.Style.BorderColorHorizontal);
                    PrintRowColor("BorderColorVertical", node.Style.BorderColorVertical);
                    if (node.Style.Direction != YGDirection.Inherit) PrintRow("Direction", node.Style.Direction);
                    if (node.Style.Display != YGDisplay.Flex) PrintRow("Display", node.Style.Display);
                    if (node.Style.PositionType != YGPositionType.Relative) PrintRow("PositionType", node.Style.PositionType);
                    PrintRow("PositionTop", node.Style.PositionTop);
                    PrintRow("PositionLeft", node.Style.PositionLeft);
                    PrintRow("Flex", node.Style.Flex);
                    PrintRow("FlexBasis", node.Style.FlexBasis);
                    if (node.Style.FlexDirection != YGFlexDirection.Column) PrintRow("FlexDirection", node.Style.FlexDirection);
                    PrintRow("FlexGrow", node.Style.FlexGrow);
                    PrintRow("FlexShrink", node.Style.FlexShrink);
                    if (node.Style.FlexWrap != YGWrap.NoWrap) PrintRow("FlexWrap", node.Style.FlexWrap);
                    PrintRow("Gap", node.Style.Gap);
                    PrintRow("GapColumn", node.Style.GapColumn);
                    PrintRow("GapRow", node.Style.GapRow);
                    PrintRow("MinHeight", node.Style.MinHeight);
                    PrintRow("MinWidth", node.Style.MinWidth);
                    PrintRow("Height", node.Style.Height);
                    PrintRow("Width", node.Style.Width);
                    if (node.Style.JustifyContent != YGJustify.FlexStart) PrintRow("JustifyContent", node.Style.JustifyContent);
                    PrintRow("Margin", node.Style.Margin);
                    PrintRow("MarginTop", node.Style.MarginTop);
                    PrintRow("MarginRight", node.Style.MarginRight);
                    PrintRow("MarginBottom", node.Style.MarginBottom);
                    PrintRow("MarginLeft", node.Style.MarginLeft);
                    PrintRow("MarginHorizontal", node.Style.MarginHorizontal);
                    PrintRow("MarginVertical", node.Style.MarginVertical);
                    if (node.Style.Overflow != YGOverflow.Visible) PrintRow("Overflow", node.Style.Overflow);
                    PrintRow("Padding", node.Style.Padding);
                    PrintRow("PaddingTop", node.Style.PaddingTop);
                    PrintRow("PaddingRight", node.Style.PaddingRight);
                    PrintRow("PaddingBottom", node.Style.PaddingBottom);
                    PrintRow("PaddingLeft", node.Style.PaddingLeft);
                    PrintRow("PaddingHorizontal", node.Style.PaddingHorizontal);
                    PrintRow("PaddingVertical", node.Style.PaddingVertical);
                }
            }
        }

        using (var tab = ImRaii.TabItem("Computed"))
        {
            if (tab)
            {
                using var _table = ImRaii.Table("ComputedTable", 2);
                if (_table)
                {
                    PrintRow("ComputedTop", node.ComputedTop);
                    PrintRow("ComputedLeft", node.ComputedLeft);
                    PrintRow("ComputedPosition", node.ComputedPosition);
                    PrintRow("CumulativePosition", node.CumulativePosition);
                    PrintRow("ComputedSize", node.ComputedSize);
                    PrintRow("ComputedBorderTop", node.ComputedBorderTop);
                    PrintRow("ComputedBorderRight", node.ComputedBorderRight);
                    PrintRow("ComputedBorderBottom", node.ComputedBorderBottom);
                    PrintRow("ComputedBorderLeft", node.ComputedBorderLeft);
                    PrintRow("ComputedWidth", node.ComputedWidth);
                    PrintRow("ComputedHeight", node.ComputedHeight);
                }
            }
        }
    }

    private void PrintRow(string text, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(text);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(value);
        //ImGui.InputText($"##{text}", ref value, (uint)value.Length, ImGuiInputTextFlags.ReadOnly);
    }

    private void PrintRow<T>(string text, T value) where T : Enum
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(text);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }

    private void PrintRow(string text, bool value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(text);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }

    private void PrintRowColor(string text, HaselColor? value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(text);
        ImGui.TableNextColumn();

        if (value == null)
        {
            ImGui.TextUnformatted("null");
            return;
        }

        var vec = (Vector4)value;
        // TODO: maybe one day we get an updated imgui version with ImGuiItemFlags.ReadOnly
        ImGui.ColorEdit4(text, ref vec, ImGuiColorEditFlags.NoLabel);
    }

    private void PrintRow(string text, float value)
    {
        if (float.IsNaN(value))
            return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(text);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }

    private void PrintRow(string text, Vector2 value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(text);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }

    private void PrintRow(string text, YGValue value)
    {
        if (value.unit is YGUnit.Undefined)
            return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(text);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{value}");
    }
}
