using Dalamud.Plugin;
using HaselCommon.Services;

namespace HaselCommon;

public static class HaselCommonBase
{
    public static AddonObserver AddonObserver { get; private set; } = null!;
    public static TranslationManager TranslationManager { get; private set; } = null!;
    public static StringManager StringManager { get; private set; } = null!;
    public static TextureManager TextureManager { get; private set; } = null!;
    public static WindowManager WindowManager { get; private set; } = null!;

    public static void Initialize(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        AddonObserver = new();
        TranslationManager = new(pluginInterface, Service.ClientState);
        StringManager = new();
        TextureManager = new(Service.Framework);
        WindowManager = new(pluginInterface);
    }

    public static void Dispose()
    {
        AddonObserver.Dispose();
        TranslationManager.Dispose();
        TextureManager.Dispose();
        WindowManager.Dispose();
    }
}
