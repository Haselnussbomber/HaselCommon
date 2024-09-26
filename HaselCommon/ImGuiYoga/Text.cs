using System.Text;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using HaselCommon.ImGuiYoga.Events;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Text.ReadOnly;
using YogaSharp;
using YGMeasureFuncDelegate = YogaSharp.Interop.YGMeasureFuncDelegate;

namespace HaselCommon.ImGuiYoga;

// add some compiling mechanism when https://github.com/goatcorp/Dalamud/pull/2033 is merged

#pragma warning disable SeStringRenderer
public unsafe class Text : CharacterData
{
    private bool isSeString = false;

    public override string AsHtmlOpenTag => TextValue.Replace('\n', ' ');

    public Text() : base()
    {
        SetMeasureFunc(new YGMeasureFuncDelegate(Measure));
        NodeType = NodeType.Text;
    }

    public Text(ReadOnlySeString data) : this()
    {
        Data = data;
    }

    public Text(string data) : this()
    {
        Data = new ReadOnlySeString(Encoding.UTF8.GetBytes(data)); // TODO: use ReadOnlySeString.FromText() when https://github.com/goatcorp/Dalamud/pull/2033 is merged
    }

    public string TextValue
    {
        get => Data.ExtractText();
        set => Data = Encoding.UTF8.GetBytes(value);
    }

    public bool IsSeString
    {
        get => isSeString;
        set
        {
            if (isSeString != value)
            {
                isSeString = value;
                IsDirty = true;
            }
        }
    }

    public SeStringDrawParams DrawParams { get; set; } = default;

    private SeStringDrawParams TextDrawParams => DrawParams with
    {
        Color = ComputedStyle.Color,
        FontSize = ComputedStyle.FontSize,
    };

    private YGSize Measure(YGNode* node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        using var font = FontHandle?.Push();

        return IsSeString
            ? ImGuiHelpers.SeStringWrapped(Data, TextDrawParams with { TargetDrawList = 0, WrapWidth = width }).Size
            : ImGui.CalcTextSize(Data.ExtractText(), width);
    }

    protected override void DrawNode()
    {
        if (!ImGuiUtils.IsInViewport(ComputedSize))
        {
            ImGui.Dummy(ComputedSize);
            return;
        }

        using var font = FontHandle?.Push();

        if (IsSeString)
        {
            RenderSeString();
        }
        else
        {
            RenderString();
        }
    }

    private void RenderSeString()
    {
        var result = ImGuiHelpers.SeStringWrapped(Data, TextDrawParams with { WrapWidth = ComputedWidth });

        if (result.Clicked && !result.InteractedPayloadEnvelope.IsEmpty)
        {
            DispatchEvent(new MouseEvent()
            {
                Sender = this,
                EventType = MouseEventType.MouseClick,
                Payload = new ReadOnlySeString(result.InteractedPayloadEnvelope.ToArray())
            });
        }
    }

    private void RenderString()
    {
        ImGui.PushTextWrapPos(CumulativePosition.X + ComputedWidth);

        using (ComputedStyle.Color.Push(ImGuiCol.Text))
            ImGui.TextUnformatted(Data.ExtractText());

        ImGui.PopTextWrapPos();
    }
}
