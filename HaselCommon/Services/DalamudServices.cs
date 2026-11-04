using HaselCommon.Logger;

namespace HaselCommon.Services;

public static class DalamudServices
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddSingleton(provider
            => provider.GetRequiredService<IDalamudPluginInterface>().UiBuilder);

        services.AddSingleton(provider
            => provider.GetRequiredService<IUiBuilder>().FontAtlas);

        services.AddSingleton<ILoggerFactory>(provider
            => new DalamudLoggerFactory(provider.GetRequiredService<IPluginLog>()));

        services.AddTransient(typeof(ILogger<>), typeof(Logger<>));
    }
}
