using Dalamud.IoC;

namespace HaselCommon.Utils.Internal;

internal class DalamudServiceWrapper<T>
{
    [PluginService] internal T Service { get; private set; } = default!;

    internal DalamudServiceWrapper(IDalamudPluginInterface pi)
    {
        pi.Inject(this);
    }
}
