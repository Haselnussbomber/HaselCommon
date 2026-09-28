using HaselCommon.Logger;

namespace HaselCommon.Services;

public static class DalamudServices
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        var serviceType = typeof(IDalamudService);
        foreach (var type in serviceType.Assembly.ExportedTypes.Where(t => serviceType.IsAssignableFrom(t)))
        {
            if (services.Any(t => t.ServiceType == type))
                continue;

            services.AddSingleton(type, provider
                => provider.GetRequiredService<IDalamudPluginInterface>().GetRequiredService(type));
        }

        services.AddSingleton(provider
            => provider.GetRequiredService<IDalamudPluginInterface>().UiBuilder);

        services.AddSingleton(provider
            => provider.GetRequiredService<IUiBuilder>().FontAtlas);

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.Services.AddSingleton<ILoggerProvider>(provider
                => new DalamudLoggerProvider(provider.GetRequiredService<IPluginLog>()));
        });
    }
}
