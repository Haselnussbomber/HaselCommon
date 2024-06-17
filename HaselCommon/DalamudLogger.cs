using System.Collections.Concurrent;
using System.Linq;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;

namespace HaselCommon;

public sealed class DalamudLogger(string name, IPluginLog pluginLog) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = $"[{name}] {formatter(state, exception)}";

        switch (logLevel)
        {
            case LogLevel.Trace:
                pluginLog.Verbose(message);
                break;

            case LogLevel.Debug:
                pluginLog.Debug(message);
                break;

            case LogLevel.Information:
                pluginLog.Information(message);
                break;

            case LogLevel.Warning:
                pluginLog.Warning(message);
                break;

            case LogLevel.Error when exception is not null:
                pluginLog.Error(exception, message);
                break;

            case LogLevel.Error:
                pluginLog.Error(message);
                break;

            case LogLevel.Critical:
                pluginLog.Error(message);
                break;

            case LogLevel.None:
                pluginLog.Information(message);
                break;
        }
    }
}

public sealed class DalamudLoggerProvider(IPluginLog pluginLog) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, DalamudLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string moduleName)
        => _loggers.GetOrAdd(moduleName.Split('.').Last(), name => new DalamudLogger(name, pluginLog));

    public void Dispose()
    {
        _loggers.Clear();
    }
}
