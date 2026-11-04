using System.Collections.Concurrent;

namespace HaselCommon.Logger;

public class DalamudLoggerFactory(IPluginLog pluginLog) : ILoggerFactory
{
    private readonly ConcurrentDictionary<string, DalamudLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName.Split('.').Last(), name => new DalamudLogger(name, pluginLog));

    public void AddProvider(ILoggerProvider provider) { }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
