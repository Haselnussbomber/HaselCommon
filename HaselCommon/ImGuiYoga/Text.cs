using System.Text;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;
using HaselCommon.ImGuiYoga.Attributes;
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
    private bool fontDirty;
    private IFontHandle? fontHandle;

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

    public override void Dispose()
    {
        DisposeFontHandle();
        base.Dispose();
    }

    private void DisposeFontHandle()
    {
        if (fontHandle != null)
        {
            fontHandle.ImFontChanged -= FontHandle_ImFontChanged;
            fontHandle.Dispose();
        }
    }

    [NodeProperty("sestring")]
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

    public void GenerateFont()
    {
        var fontAtlas = Service.Get<IDalamudPluginInterface>().UiBuilder.FontAtlas;
        var size = Style.FontSize.Resolve();
        var fontStyle = Style.FontName.Resolve().ToLowerInvariant() switch
        {
            "jupiter" => new GameFontStyle(GameFontFamily.Jupiter, size),
            "jupiternumeric" => new GameFontStyle(GameFontFamily.JupiterNumeric, size),
            "meidinger" => new GameFontStyle(GameFontFamily.Meidinger, size),
            "miedingermid" => new GameFontStyle(GameFontFamily.MiedingerMid, size),
            "trumpgothic" => new GameFontStyle(GameFontFamily.TrumpGothic, size),
            _ => new GameFontStyle(GameFontFamily.Axis, size),
        };

        DisposeFontHandle();
        fontHandle = fontAtlas.NewGameFontHandle(fontStyle);
        fontHandle.ImFontChanged += FontHandle_ImFontChanged;
    }

    private void FontHandle_ImFontChanged(IFontHandle fontHandle, ILockedImFont lockedFont)
    {
        IsDirty = true;
    }

    private SeStringDrawParams TextDrawParams => DrawParams with
    {
        Color = Style.Color.Resolve(),
        FontSize = Style.FontSize.Resolve(),
    };

    private YGSize Measure(YGNode* node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        if (fontDirty)
        {
            GenerateFont();
            fontDirty = false;
        }

        using var font = fontHandle?.Push();

        return IsSeString
            ? ImGuiHelpers.SeStringWrapped(Data, TextDrawParams with { TargetDrawList = 0, WrapWidth = width }).Size
            : ImGui.CalcTextSize(Data.ExtractText(), width);
    }

    protected override void DrawNode()
    {
        if (fontDirty)
            IsDirty = true;

        if (!ImGuiUtils.IsInViewport(ComputedSize))
        {
            ImGui.Dummy(ComputedSize);
            return;
        }

        using var font = fontHandle?.Push();

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

        using (Style.Color.Resolve().Push(ImGuiCol.Text))
            ImGui.TextUnformatted(Data.ExtractText());

        ImGui.PopTextWrapPos();
    }
}
