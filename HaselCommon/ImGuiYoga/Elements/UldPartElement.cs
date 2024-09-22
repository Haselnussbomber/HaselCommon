using HaselCommon.ImGuiYoga.Attributes;
using HaselCommon.Services;

namespace HaselCommon.ImGuiYoga.Elements;

public class UldPartElement : Node
{
    private TextureService? textureService;

    [NodeProperty("uld")]
    public required string UldName { get; set; }

    [NodeProperty("part-list-id")]
    public required uint PartListId { get; set; }

    [NodeProperty("part-index")]
    public required uint PartIndex { get; set; }

    // TODO: measure default size somehow

    protected override void DrawNode()
    {
        textureService ??= Service.Get<TextureService>();
        textureService.DrawPart(UldName, PartListId, PartIndex, new DrawInfo(ComputedSize));
    }
}
