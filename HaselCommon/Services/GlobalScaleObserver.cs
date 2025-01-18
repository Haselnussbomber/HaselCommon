using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;

namespace HaselCommon.Services;

[RegisterSingleton]
public class GlobalScaleObserver : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private float _globalScale;

    public event Action<float>? ScaleChange;

    public GlobalScaleObserver(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
        _pluginInterface.UiBuilder.FontAtlas.BuildStepChange += OnBuildStepChange;
    }

    public void Dispose()
    {
        _pluginInterface.UiBuilder.FontAtlas.BuildStepChange -= OnBuildStepChange;
        GC.SuppressFinalize(this);
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
