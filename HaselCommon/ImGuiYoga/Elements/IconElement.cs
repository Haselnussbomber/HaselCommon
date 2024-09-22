using HaselCommon.ImGuiYoga.Attributes;
using HaselCommon.Services;

namespace HaselCommon.ImGuiYoga.Elements;

public class IconElement : Node
{
    private TextureService? textureService;

    [NodeProperty("icon-id")]
    public uint IconId { get; set; }

    // TODO: measure default size somehow

    protected override void DrawNode()
    {
        textureService ??= Service.Get<TextureService>();
        textureService.DrawIcon(IconId, new DrawInfo(ComputedSize));
    }
}
