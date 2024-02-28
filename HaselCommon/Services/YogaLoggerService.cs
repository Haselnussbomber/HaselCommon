using Dalamud.Memory;
using HaselCommon.Yoga;
using YGLoggerDelegate = HaselCommon.Yoga.Interop.YGLoggerDelegate;

namespace HaselCommon.Services;

public unsafe class YogaLoggerService : IDisposable
{
    private GCHandle LoggerDelegateHandle;

    public YogaLoggerService()
    {
        var loggerDelegate = new YGLoggerDelegate(Logger);
        LoggerDelegateHandle = GCHandle.Alloc(loggerDelegate);
        YGConfig.GetDefault()->SetLogger(Marshal.GetFunctionPointerForDelegate(loggerDelegate));
    }

    public void Dispose()
    {
        LoggerDelegateHandle.Free();
    }

    private static int Logger(YGConfig* config, YGNode* node, LogLevel level, char* format, /* va_list */ char** args)
    {
        //var formatStr = MemoryHelper.ReadStringNullTerminated((nint)format).TrimEnd(); // only "%s\n" is used
        var argsStr = MemoryHelper.ReadStringNullTerminated(*(nint*)args);
        switch (level)
        {
            case LogLevel.Error:
                Service.PluginLog.Error($"[Yoga] {argsStr}");
                break;
            case LogLevel.Warn:
                Service.PluginLog.Warning($"[Yoga] {argsStr}");
                break;
            case LogLevel.Info:
                Service.PluginLog.Info($"[Yoga] {argsStr}");
                break;
            case LogLevel.Debug:
                Service.PluginLog.Debug($"[Yoga] {argsStr}");
                break;
            case LogLevel.Verbose:
                Service.PluginLog.Verbose($"[Yoga] {argsStr}");
                break;
            case LogLevel.Fatal:
                Service.PluginLog.Error($"[Yoga] {argsStr}");
                break;
        }
        return 0;
    }
}
