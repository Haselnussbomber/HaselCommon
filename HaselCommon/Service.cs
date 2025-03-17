using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Logger;
using HaselCommon.Services;
using HaselCommon.Utils.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HaselCommon;

public static class Service
{
    public static IServiceCollection Collection { get; set; } = new ServiceCollection();
    public static ServiceProvider? Provider { get; private set; }

    public static void Initialize(Action? callback = null)
    {
        Provider = Collection.BuildServiceProvider();

        Get<IEnumerable<IEventProvider>>();

        if (callback != null)
        {
            Get<IFramework>().RunOnFrameworkThread(callback);
        }
    }

    public static void Dispose()
        => Provider?.Dispose();

    public static T Get<T>() where T : notnull
        => Provider!.GetRequiredService<T>();

    public static bool TryGet<T>([NotNullWhen(returnValue: true)] out T? service)
    {
        if (Provider == null)
        {
            service = default;
            return false;
        }

        try
        {
            service = Provider.GetService<T>();
            return service != null;
        }
        catch // might catch ObjectDisposedException here
        {
            service = default;
            return false;
        }
    }

    public static IServiceCollection AddDalamud(this IServiceCollection collection, IDalamudPluginInterface pluginInterface)
    {
        T DalamudServiceFactory<T>(IServiceProvider serviceProvider) => new DalamudServiceWrapper<T>(pluginInterface).Service;

        collection
            .AddSingleton(new PluginAssemblyProvider(Assembly.GetCallingAssembly()))
            .AddSingleton(pluginInterface)
            //.AddSingleton(DalamudServiceFactory<IAddonEventManager>)
            .AddSingleton(DalamudServiceFactory<IAddonLifecycle>)
            //.AddSingleton(DalamudServiceFactory<IAetheryteList>)
            //.AddSingleton(DalamudServiceFactory<IBuddyList>)
            //.AddSingleton(DalamudServiceFactory<IChatGui>)
            .AddSingleton(DalamudServiceFactory<IClientState>)
            .AddSingleton(DalamudServiceFactory<ICommandManager>)
            //.AddSingleton(DalamudServiceFactory<ICondition>)
            .AddSingleton(DalamudServiceFactory<IContextMenu>)
            .AddSingleton(DalamudServiceFactory<IDataManager>)
            .AddSingleton(DalamudServiceFactory<IDtrBar>)
            //.AddSingleton(DalamudServiceFactory<IDutyState>)
            //.AddSingleton(DalamudServiceFactory<IFateTable>)
            //.AddSingleton(DalamudServiceFactory<IFlyTextGui>)
            .AddSingleton(DalamudServiceFactory<IFramework>)
            .AddSingleton(DalamudServiceFactory<IGameConfig>)
            .AddSingleton(DalamudServiceFactory<IGameGui>)
            .AddSingleton(DalamudServiceFactory<IGameInteropProvider>)
            .AddSingleton(DalamudServiceFactory<IGameInventory>)
            .AddSingleton(DalamudServiceFactory<IGameLifecycle>)
            //.AddSingleton(DalamudServiceFactory<IGameNetwork>)
            //.AddSingleton(DalamudServiceFactory<IGamepadState>)
            //.AddSingleton(DalamudServiceFactory<IJobGauges>)
            .AddSingleton(DalamudServiceFactory<IKeyState>)
            .AddSingleton(DalamudServiceFactory<IMarketBoard>)
            .AddSingleton(DalamudServiceFactory<INotificationManager>)
            //.AddSingleton(DalamudServiceFactory<IObjectTable>)
            //.AddSingleton(DalamudServiceFactory<IPartyFinderGui>)
            //.AddSingleton(DalamudServiceFactory<IPartyList>)
            .AddSingleton(DalamudServiceFactory<IPluginLog>)
            .AddSingleton(DalamudServiceFactory<ISigScanner>)
            //.AddSingleton(DalamudServiceFactory<ITargetManager>)
            .AddSingleton(DalamudServiceFactory<ITextureProvider>)
            //.AddSingleton(DalamudServiceFactory<ITextureReadbackProvider>)
            //.AddSingleton(DalamudServiceFactory<ITextureSubstitutionProvider>)
            //.AddSingleton(DalamudServiceFactory<ITitleScreenMenu>)
            //.AddSingleton(DalamudServiceFactory<IToastGui>)
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.Services.AddSingleton<ILoggerProvider>(serviceProvider =>
                {
                    var pluginLog = serviceProvider.GetRequiredService<IPluginLog>();
                    return new DalamudLoggerProvider(pluginLog);
                });
            });

        return collection;
    }
}
