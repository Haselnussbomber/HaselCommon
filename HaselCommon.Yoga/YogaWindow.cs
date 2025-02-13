using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.Yoga;

public partial class YogaWindow : SimpleWindow, IDisposable
{
    private bool _showNodeInspector;
    private bool _fontUpdated;
    private Vector2 _contentRegionAvail;
    private Vector2 _cursorPosition;

    public Node RootNode { get; } = [];
    public bool ShowNodeInspector
    {
        get => _showNodeInspector;
        set => _showNodeInspector = value;
    }

    public YogaWindow(WindowManager wm, TextService textService, LanguageProvider languageProvider) : base(wm, textService, languageProvider)
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

    public override void OnScaleChange(float scale)
    {
        _fontUpdated = true;
    }

    public override void Dispose()
    {
        RootNode.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        var contentRegionAvail = ImGui.GetContentRegionAvail();
        var cursorPosition = ImGui.GetCursorPos();

        if (_fontUpdated || _contentRegionAvail != contentRegionAvail || _cursorPosition != cursorPosition)
        {
            RootNode.PositionLeft = cursorPosition.X;
            RootNode.PositionTop = cursorPosition.Y;
            RootNode.Width = contentRegionAvail.X;
            RootNode.Height = contentRegionAvail.Y;

            _fontUpdated = false;
            _contentRegionAvail = contentRegionAvail;
            _cursorPosition = cursorPosition;

            ApplyGlobalScale(ImGuiHelpers.GlobalScale);
        }

#if DEBUG
        if (ShowNodeInspector)
        {
            _debugTimer.Restart();
            RootNode.Update();
            _debugTimer.Stop();
            _debugUpdateTime = _debugTimer.Elapsed.TotalMilliseconds;

            _debugTimer.Restart();
            RootNode.CalculateLayout(contentRegionAvail);
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
            RootNode.CalculateLayout(contentRegionAvail);
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
