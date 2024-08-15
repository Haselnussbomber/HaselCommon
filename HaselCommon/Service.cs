using System.Reflection;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Services;
using HaselCommon.Services.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace HaselCommon;

public static class Service
{
    internal static Assembly PluginAssembly { get; private set; } = null!;

    public static IServiceCollection Collection { get; } = new ServiceCollection();
    public static ServiceProvider? Provider { get; private set; }

    public static void BuildProvider()
        => Provider = Collection.BuildServiceProvider();

    public static void Dispose()
        => Provider?.Dispose();

    public static T Get<T>() where T : notnull
        => Provider!.GetRequiredService<T>();

    public static IServiceCollection Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginAssembly = Assembly.GetCallingAssembly();
        AddDefaultServices(pluginInterface);
        return Collection;
    }

    private static void AddDefaultServices(IDalamudPluginInterface pi)
    {
        T DalamudServiceFactory<T>(IServiceProvider serviceProvider) => new DalamudServiceWrapper<T>(pi).Service;

        Collection
            // Dalamud
            .AddSingleton(pi)
            .AddSingleton(DalamudServiceFactory<IAddonEventManager>)
            .AddSingleton(DalamudServiceFactory<IAddonLifecycle>)
            .AddSingleton(DalamudServiceFactory<IAetheryteList>)
            .AddSingleton(DalamudServiceFactory<IBuddyList>)
            .AddSingleton(DalamudServiceFactory<IChatGui>)
            .AddSingleton(DalamudServiceFactory<IClientState>)
            .AddSingleton(DalamudServiceFactory<ICommandManager>)
            .AddSingleton(DalamudServiceFactory<ICondition>)
            .AddSingleton(DalamudServiceFactory<IContextMenu>)
            .AddSingleton(DalamudServiceFactory<IDataManager>)
            .AddSingleton(DalamudServiceFactory<IDtrBar>)
            .AddSingleton(DalamudServiceFactory<IDutyState>)
            .AddSingleton(DalamudServiceFactory<IFateTable>)
            .AddSingleton(DalamudServiceFactory<IFlyTextGui>)
            .AddSingleton(DalamudServiceFactory<IFramework>)
            .AddSingleton(DalamudServiceFactory<IGameConfig>)
            .AddSingleton(DalamudServiceFactory<IGameGui>)
            .AddSingleton(DalamudServiceFactory<IGameInteropProvider>)
            .AddSingleton(DalamudServiceFactory<IGameInventory>)
            .AddSingleton(DalamudServiceFactory<IGameLifecycle>)
            .AddSingleton(DalamudServiceFactory<IGameNetwork>)
            .AddSingleton(DalamudServiceFactory<IGamepadState>)
            .AddSingleton(DalamudServiceFactory<IJobGauges>)
            .AddSingleton(DalamudServiceFactory<IKeyState>)
            .AddSingleton(DalamudServiceFactory<IMarketBoard>)
            .AddSingleton(DalamudServiceFactory<INotificationManager>)
            .AddSingleton(DalamudServiceFactory<IObjectTable>)
            .AddSingleton(DalamudServiceFactory<IPartyFinderGui>)
            .AddSingleton(DalamudServiceFactory<IPartyList>)
            .AddSingleton(DalamudServiceFactory<IPluginLog>)
            .AddSingleton(DalamudServiceFactory<ISigScanner>)
            .AddSingleton(DalamudServiceFactory<ITargetManager>)
            .AddSingleton(DalamudServiceFactory<ITextureProvider>)
            .AddSingleton(DalamudServiceFactory<ITextureReadbackProvider>)
            .AddSingleton(DalamudServiceFactory<ITextureSubstitutionProvider>)
            .AddSingleton(DalamudServiceFactory<ITitleScreenMenu>)
            .AddSingleton(DalamudServiceFactory<IToastGui>)

            // HaselCommon
            .AddSingleton<AddonObserver>()
            .AddSingleton<CommandService>()
            .AddSingleton<ExcelService>()
            .AddSingleton<GamepadService>()
            .AddSingleton<ImGuiContextMenuService>()
            .AddSingleton<ImGuiService>()
            .AddSingleton<ItemService>()
            .AddSingleton<LeveService>()
            .AddSingleton<MapService>()
            .AddSingleton<MarketBoardService>()
            .AddSingleton<PlayerService>()
            .AddSingleton<TeleportService>()
            .AddSingleton<TextDecoder>()
            .AddSingleton<TextService>()
            .AddSingleton<TextureService>()
            .AddSingleton<TranslationManager>()
            .AddSingleton<WindowManager>()
            .AddSingleton<SeStringEvaluatorService>();
    }
}
