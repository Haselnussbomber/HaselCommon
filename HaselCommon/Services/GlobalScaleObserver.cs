using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class GlobalScaleObserver : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;

    private float _globalScale;

    public event Action<float>? ScaleChange;

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
            ScaleChange?.Invoke(ImGuiHelpers.GlobalScaleSafe);
            _globalScale = ImGuiHelpers.GlobalScaleSafe;
        }
    }
}
