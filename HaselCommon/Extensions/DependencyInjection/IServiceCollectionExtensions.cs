using System.Reflection;
using HaselCommon.Logger;

namespace HaselCommon.Extensions;

public static class IServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddDalamud(IDalamudPluginInterface pluginInterface)
        {
            var pluginLog = pluginInterface.GetRequiredService<IPluginLog>();

            return serviceCollection
                .AddSingleton(new PluginAssemblyProvider(Assembly.GetCallingAssembly()))
                .AddSingleton(pluginLog)
                .AddSingleton(pluginInterface)
                .AddSingleton(serviceProvider => pluginInterface.UiBuilder)
                .AddSingleton(serviceProvider => pluginInterface.UiBuilder.FontAtlas)
                .AddSingleton(_ => pluginInterface.GetRequiredService<IAddonEventManager>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IAddonLifecycle>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IAetheryteList>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IBuddyList>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IChatGui>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IClientState>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ICommandManager>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ICondition>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IContextMenu>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IDataManager>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IDtrBar>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IDutyState>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IFateTable>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IFlyTextGui>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IFramework>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IGameConfig>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IGameGui>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IGameInteropProvider>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IGameInventory>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IGameLifecycle>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IGamepadState>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IJobGauges>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IKeyState>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IMarketBoard>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<INotificationManager>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IObjectTable>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IPartyFinderGui>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IPartyList>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IPlayerState>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IReliableFileStorage>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ISeStringEvaluator>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ISelfTestRegistry>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ISigScanner>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ITargetManager>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ITextureProvider>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ITextureReadbackProvider>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ITextureSubstitutionProvider>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<ITitleScreenMenu>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IToastGui>())
                .AddSingleton(_ => pluginInterface.GetRequiredService<IUnlockState>())
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.Services.AddSingleton<ILoggerProvider>(new DalamudLoggerProvider(pluginLog));
                });
        }
    }
}
