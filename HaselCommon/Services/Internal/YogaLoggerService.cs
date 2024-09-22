using Dalamud.Memory;
using Microsoft.Extensions.Logging;
using YogaSharp;
using YGLoggerDelegate = YogaSharp.Interop.YGLoggerDelegate;
using YGLogLevel = YogaSharp.YGLogLevel;

namespace HaselCommon.Services.Internal;

internal unsafe class YogaLoggerService : IDisposable
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

    private int Log(YGConfig* config, YGNode* node, YGLogLevel level, char* format, /* va_list */ char** args)
    {
        //var formatStr = MemoryHelper.ReadStringNullTerminated((nint)format).TrimEnd(); // only "%s\n" is used
        var argsStr = MemoryHelper.ReadStringNullTerminated(*(nint*)args);
        switch (level)
        {
            case YGLogLevel.Error:
                Logger.LogError(argsStr);
                break;
            case YGLogLevel.Warn:
                Logger.LogWarning(argsStr);
                break;
            case YGLogLevel.Info:
                Logger.LogInformation(argsStr);
                break;
            case YGLogLevel.Debug:
                Logger.LogDebug(argsStr);
                break;
            case YGLogLevel.Verbose:
                Logger.LogTrace(argsStr);
                break;
            case YGLogLevel.Fatal:
                Logger.LogError(argsStr);
                break;
        }
        return 0;
    }
}
