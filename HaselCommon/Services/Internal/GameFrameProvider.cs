using FFXIVClientStructs.FFXIV.Client.System.Framework;
using R3;
using R3.Collections;

namespace HaselCommon.Services;

[RegisterSingleton<FrameProvider>, AutoConstruct]
internal partial class GameFrameProvider : FrameProvider, IDisposable
{
    private readonly object _gate = new();
    private readonly IFramework _framework;
    private FreeListCore<IFrameRunnerWorkItem> _list;

    [AutoPostConstruct]
    private void Initialize()
    {
        _list = new FreeListCore<IFrameRunnerWorkItem>(_gate);
        _framework.Update += OnFrameworkUpdate;
    }

    public override unsafe long GetFrameCount()
    {
        return Framework.Instance()->FrameCounter;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        _list.Add(callback, out _);
    }

    private unsafe void OnFrameworkUpdate(IFramework _)
    {
        var frameCount = Framework.Instance()->FrameCounter;
        var span = _list.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            ref readonly var item = ref span[i];
            if (item != null)
            {
                try
                {
                    if (!item.MoveNext(frameCount))
                    {
                        _list.Remove(i);
                    }
                }
                catch (Exception ex)
                {
                    _list.Remove(i);
                    try
                    {
                        ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    }
                    catch { }
                }
            }
        }
    }

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;
        _list.Dispose();
    }
}
