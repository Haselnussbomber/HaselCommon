using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Utils;
using ImGuiNET;
using YogaSharp;

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
        ImGuiUtils.VerticalSeparator();
        ImGui.SameLine();
        if (ImGui.Button("Set Style Dirty"))
            Document.SetStyleDirty();
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
                                PrintRow(attr.Key, attr.Value ?? string.Empty);
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
                using var childFrame = ImRaii.Child("StyleChild", new Vector2(-1));
                if (!childFrame) return;

                if (node.Style.Count > 0)
                {
                    ImGui.TextUnformatted("Inline");
                    using (var _table = ImRaii.Table("StyleTable", 2))
                    {
                        if (_table)
                        {
                            foreach (var attr in node.Style)
                            {
                                PrintRow(attr.Key, attr.Value);
                            }
                        }
                    }
                }

                var i = 0;
                foreach (var (selectorText, declaration) in node.StyleDeclarations)
                {
                    if (i > 0 || node.Style.Count > 0)
                        ImGui.Separator();
                    ImGui.TextUnformatted(selectorText);
                    using (var _table = ImRaii.Table($"StyleDeclarationTable{i++}", 2))
                    {
                        if (_table)
                        {
                            foreach (var attr in declaration)
                            {
                                PrintRow(attr.Name, attr.Value);
                            }
                        }
                    }
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
                    PrintRow("FontHandle", node.FontHandle?.Available ?? false);
                    PrintRow("HadOverflow", node.ComputedStyle.HadOverflow);
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
