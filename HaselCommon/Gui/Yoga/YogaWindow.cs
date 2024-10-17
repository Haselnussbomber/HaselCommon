using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using HaselCommon.Services;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.Gui.Yoga;

public partial class YogaWindow : SimpleWindow, IDisposable
{
    private readonly IFontHandle _defaultFontHandle;
    private bool _showNodeInspector;
    private bool _fontUpdated = true;

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

        _defaultFontHandle = Service.Get<IDalamudPluginInterface>().UiBuilder.DefaultFontHandle;
        _defaultFontHandle.ImFontChanged += DefaultFontHandle_ImFontChanged;
    }

    private void DefaultFontHandle_ImFontChanged(IFontHandle fontHandle, ILockedImFont lockedFont)
    {
        _fontUpdated = true;
    }

    public override void Dispose()
    {
        _defaultFontHandle.ImFontChanged -= DefaultFontHandle_ImFontChanged;
        RootNode.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        if (_fontUpdated)
        {
            RootNode.PositionTop = ImGui.GetCursorPosY();
            RootNode.PositionLeft = ImGui.GetCursorPosX();
            RootNode.Width = ImGui.GetContentRegionAvail().X;
            RootNode.Height = ImGui.GetContentRegionAvail().Y;

            _fontUpdated = false;

            ApplyGlobalScale(ImGui.GetIO().FontGlobalScale);
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

    private void ApplyGlobalScale(float globalFontScale)
    {
        RootNode.Traverse(node => node.ApplyGlobalScale(globalFontScale));
    }
}
