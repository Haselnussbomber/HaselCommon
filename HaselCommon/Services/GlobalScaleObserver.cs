using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;

namespace HaselCommon.Services;

[RegisterSingleton]
public class GlobalScaleObserver : IDisposable
{
    private readonly IFontHandle _defaultFontHandle;

    public event Action<float>? ScaleChange;

    public GlobalScaleObserver(IDalamudPluginInterface pluginInterface)
    {
        _defaultFontHandle = pluginInterface.UiBuilder.DefaultFontHandle;
        _defaultFontHandle.ImFontChanged += OnDefaultFontChanged;
    }

    public void Dispose()
    {
        _defaultFontHandle.ImFontChanged -= OnDefaultFontChanged;
        GC.SuppressFinalize(this);
    }

    private void OnDefaultFontChanged(IFontHandle fontHandle, ILockedImFont lockedFont)
    {
        ScaleChange?.Invoke(ImGuiHelpers.GlobalScale);
    }
}
