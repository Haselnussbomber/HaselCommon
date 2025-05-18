using System.Collections.Concurrent;

namespace HaselCommon.Logger;

public class DalamudLoggerProvider(IPluginLog pluginLog) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, DalamudLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string moduleName)
        => _loggers.GetOrAdd(moduleName.Split('.').Last(), name => new DalamudLogger(name, pluginLog));

    public void Dispose()
    {
        _loggers.Clear();
    }
}
