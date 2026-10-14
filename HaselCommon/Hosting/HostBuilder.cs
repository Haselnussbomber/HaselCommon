namespace HaselCommon.Hosting;

public readonly struct HostBuilder()
{
    private readonly ServiceCollection _services = new();

    public HostBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        configure(_services);
        return this;
    }

    public Host Build()
    {
        var provider = _services.BuildServiceProvider();
        return new Host(provider);
    }
}
