namespace HaselCommon.Windows;

[AutoConstruct]
public partial class SimpleDebugWindow : SimpleWindow
{
    private readonly List<ISimpleDebugTab> _tabs = [];
    private ISimpleDebugTab? _selectedTab;

    [AutoPostConstruct]
    private void Initialize()
    {
        Size = new Vector2(900, 600);
        SizeCondition = ImGuiCond.Appearing;
        RegisterTab(new WindowManagerTab());
    }

    public void RegisterTab(ISimpleDebugTab tab)
    {
        _tabs.Add(tab);
    }

    public void UnregisterTab(ISimpleDebugTab tab)
    {
        _tabs.Remove(tab);
    }

    public override void Draw()
    {
        DrawSidebar();
        ImGui.SameLine();
        DrawTab();
    }

    private void DrawSidebar()
    {
        using var child = ImRaii.Child("##Sidebar", new Vector2(260 * ImGuiHelpers.GlobalScale, -1), true);
        if (!child)
            return;

        using var table = ImRaii.Table("##SidebarTable", 1, ImGuiTableFlags.NoSavedSettings);
        if (!table)
            return;

        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);

        foreach (var tab in _tabs)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (ImGui.Selectable(tab.Name, _selectedTab == tab))
            {
                _selectedTab = tab;
            }
        }
    }

    private void DrawTab()
    {
        using var child = ImRaii.Child("##Tab", new Vector2(-1), true);
        if (!child)
            return;

        _selectedTab?.Draw();
    }

    private class WindowManagerTab : ISimpleDebugTab
    {
        public string Name => "WindowManager";

        public void Draw()
        {
            using var table = ImRaii.Table("WindowManagerTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.NoSavedSettings);
            if (!table)
                return;

            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("State", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableHeadersRow();

            foreach (var window in ServiceLocator.GetService<WindowManager>()!.Windows)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var name = window.WindowName.Until("#");
                using (Color.Red.Push(ImGuiCol.Text, window.WindowName == window.WindowNameKey))
                    ImGuiUtils.DrawCopyableText(name);
                ImGui.TableNextColumn();
                var id = window.WindowName.After("#").TrimStart('#');
                if (id != name)
                    ImGuiUtils.DrawCopyableText(id);
                ImGui.TableNextColumn();
                ImGui.Text(window.IsOpen ? "Open" : "Closed");
            }
        }
    }
}

public interface ISimpleDebugTab
{
    string Name { get; }
    void Draw();
}
