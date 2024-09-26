using HaselCommon.ImGuiYoga.Events;
using HaselCommon.Services;

namespace HaselCommon.ImGuiYoga.Elements;

public class UldPartElement : Node
{
    public override string TagName => "uld-part";

    private TextureService? textureService;
    private uint _partListId;
    private uint _partIndex;

    public required string UldName
    {
        get => Attributes["uld"] ?? string.Empty;
        set => Attributes["uld"] = value;
    }

    public required uint PartListId
    {
        get => _partListId;
        set => Attributes["part-list-id"] = value.ToString();
    }

    public required uint PartIndex
    {
        get => _partIndex;
        set => Attributes["part-index"] = value.ToString();
    }

    public override void DispatchEvent(Event evt)
    {
        if (evt is AttributeChangedEvent attributeChangedEvent)
        {
            switch (attributeChangedEvent.Name)
            {
                case "part-list-id":
                    _partListId = uint.Parse(attributeChangedEvent.Value ?? "0");
                    break;

                case "part-index":
                    _partIndex = uint.Parse(attributeChangedEvent.Value ?? "0");
                    break;
            }
        }

        base.DispatchEvent(evt);
    }

    // TODO: measure default size somehow

    protected override void DrawNode()
    {
        textureService ??= Service.Get<TextureService>();
        textureService.DrawPart(UldName, PartListId, PartIndex, new DrawInfo(ComputedSize));
    }
}
