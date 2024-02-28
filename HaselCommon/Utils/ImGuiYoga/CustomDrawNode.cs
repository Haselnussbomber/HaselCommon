namespace HaselCommon.Utils.ImGuiYoga;

public unsafe class CustomDrawNode : Node
{
    private Action<Node>? drawFunction = null;

    public CustomDrawNode(YogaWindow window, Action<Node> drawFunction) : base(window)
    {
        SetDrawFunction(drawFunction);
    }

    public void SetDrawFunction(Action<Node>? drawFunction)
    {
        this.drawFunction = drawFunction;
    }

    public override void Draw()
    {
        PreDraw();
        drawFunction?.Invoke(this);
        PostDraw();
    }
}
