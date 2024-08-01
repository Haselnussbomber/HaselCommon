using Dalamud.Memory;
using HaselCommon.Yoga;
using Microsoft.Extensions.Logging;
using LogLevel = HaselCommon.Yoga.LogLevel;
using YGLoggerDelegate = HaselCommon.Yoga.Interop.YGLoggerDelegate;

namespace HaselCommon.Services;

public unsafe class YogaLoggerService : IDisposable
{
    private readonly ILogger<YogaLoggerService> Logger;
    private GCHandle LoggerDelegateHandle;

    public YogaLoggerService(ILogger<YogaLoggerService> logger)
    {
        var loggerDelegate = new YGLoggerDelegate(Log);
        LoggerDelegateHandle = GCHandle.Alloc(loggerDelegate);
        YGConfig.GetDefault()->SetLogger(Marshal.GetFunctionPointerForDelegate(loggerDelegate));
        Logger = logger;
    }

    public void Dispose()
    {
        LoggerDelegateHandle.Free();
    }

    private int Log(YGConfig* config, YGNode* node, LogLevel level, char* format, /* va_list */ char** args)
    {
        //var formatStr = MemoryHelper.ReadStringNullTerminated((nint)format).TrimEnd(); // only "%s\n" is used
        var argsStr = MemoryHelper.ReadStringNullTerminated(*(nint*)args);
        switch (level)
        {
            case LogLevel.Error:
                Logger.LogError(argsStr);
                break;
            case LogLevel.Warn:
                Logger.LogWarning(argsStr);
                break;
            case LogLevel.Info:
                Logger.LogInformation(argsStr);
                break;
            case LogLevel.Debug:
                Logger.LogDebug(argsStr);
                break;
            case LogLevel.Verbose:
                Logger.LogTrace(argsStr);
                break;
            case LogLevel.Fatal:
                Logger.LogError(argsStr);
                break;
        }
        return 0;
    }
}
