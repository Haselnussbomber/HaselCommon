namespace HaselCommon.ImGuiYoga.Elements;

public class BrElement : Node
{
    public override string TagName => "br";

    protected override void DrawNode()
    {
        // ImGui will automatically add a new line after an item, so we don't need to do anything here
    }
}
