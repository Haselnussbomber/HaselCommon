using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Services;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.Gui.Yoga;

public partial class YogaWindow : SimpleWindow, IDisposable
{
    private bool _showNodeInspector;
    private bool _updateRootNodePosition = true;

    public Node RootNode { get; } = [];
    public bool ShowNodeInspector
    {
        get => _showNodeInspector;
        set => _showNodeInspector = value;
    }

    public YogaWindow(WindowManager wm, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(wm, name, flags, forceMainWindow)
    {
        Flags |= ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        RootNode.PositionType = YGPositionType.Absolute;
        RootNode.Overflow = YGOverflow.Scroll;

#if DEBUG
        TitleBarButtons.Add(new TitleBarButton()
        {
            Icon = FontAwesomeIcon.LayerGroup,
            IconOffset = Vector2.One,
            Click = (btn) => ShowNodeInspector = !ShowNodeInspector,
            ShowTooltip = () =>
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                using var tooltip = ImRaii.Tooltip();
                ImGui.TextUnformatted($"{(ShowNodeInspector ? "Hide" : "Show")} Node Inspector");
            }
        });
#endif
    }

    public override void Dispose()
    {
        RootNode.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        if (_updateRootNodePosition)
        {
            RootNode.PositionTop = ImGui.GetCursorPosY();
            RootNode.PositionLeft = ImGui.GetCursorPosX();
            _updateRootNodePosition = false;
        }

#if DEBUG
        if (ShowNodeInspector)
        {
            _debugTimer.Restart();
            RootNode.Update();
            _debugTimer.Stop();
            _debugUpdateTime = _debugTimer.Elapsed.TotalMilliseconds;

            _debugTimer.Restart();
            RootNode.CalculateLayout(ImGui.GetContentRegionAvail());
            _debugTimer.Stop();
            _debugLayoutTime = _debugTimer.Elapsed.TotalMilliseconds;

            _debugTimer.Restart();
            RootNode.Draw();
            _debugTimer.Stop();
            _debugDrawTime = _debugTimer.Elapsed.TotalMilliseconds;
        }
        else
        {
#endif
            RootNode.Update();
            RootNode.CalculateLayout(ImGui.GetContentRegionAvail());
            RootNode.Draw();
#if DEBUG
        }
#endif
    }

#if DEBUG
    public override void PostDraw()
    {
        base.PostDraw();

        if (ShowNodeInspector)
        {
            DrawNodeInspectorWindow();
        }
    }
#endif
}
