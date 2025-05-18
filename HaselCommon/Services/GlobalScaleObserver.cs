using Dalamud.Interface.ManagedFontAtlas;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class GlobalScaleObserver : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;

    private float _globalScale;

    public event Action<float>? ScaleChanged;

    [AutoPostConstruct]
    public void Initialize()
    {
        _pluginInterface.UiBuilder.FontAtlas.BuildStepChange += OnBuildStepChange;
    }

    public void Dispose()
    {
        _pluginInterface.UiBuilder.FontAtlas.BuildStepChange -= OnBuildStepChange;
    }

    private void OnBuildStepChange(IFontAtlasBuildToolkit toolkit)
    {
        if (toolkit.BuildStep == FontAtlasBuildStep.PostBuild && _globalScale != ImGuiHelpers.GlobalScaleSafe)
        {
            ScaleChanged?.Invoke(ImGuiHelpers.GlobalScaleSafe);
            _globalScale = ImGuiHelpers.GlobalScaleSafe;
        }
    }
}
