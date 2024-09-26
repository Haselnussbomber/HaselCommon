using HaselCommon.ImGuiYoga.Events;
using HaselCommon.Services;

namespace HaselCommon.ImGuiYoga.Elements;

public class IconElement : Node
{
    public override string TagName => "icon";

    private TextureService? textureService;
    private uint _iconId;

    public uint IconId
    {
        get => _iconId;
        set => Attributes["icon-id"] = value.ToString();
    }

    public override void DispatchEvent(Event evt)
    {
        if (evt is AttributeChangedEvent attributeChangedEvent)
        {
            switch (attributeChangedEvent.Name)
            {
                case "icon-id":
                    _iconId = uint.Parse(attributeChangedEvent.Value ?? "0");
                    break;
            }
        }

        base.DispatchEvent(evt);
    }

    protected override void DrawNode()
    {
        textureService ??= Service.Get<TextureService>();
        textureService.DrawIcon(IconId, new DrawInfo(ComputedSize));
    }
}
