namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class ImGuiService : IDisposable
{
    private readonly IUiBuilder _uiBuilder;

    public event Action? PreDraw;
    public event Action? Draw;
    public event Action? PostDraw;
    public event Action? ScaleChanged;

    [AutoPostConstruct]
    private void Initialize()
    {
        _uiBuilder.Draw += OnDraw;
        _uiBuilder.DefaultGlobalScaleChanged += OnScaleChanged;
    }

    public void Dispose()
    {
        _uiBuilder.Draw -= OnDraw;
        _uiBuilder.DefaultGlobalScaleChanged -= OnScaleChanged;
    }

    private void OnDraw()
    {
        PreDraw.InvokeSafely();
        Draw.InvokeSafely();
        PostDraw.InvokeSafely();
    }

    private void OnScaleChanged()
    {
        ScaleChanged.InvokeSafely();
    }
}
