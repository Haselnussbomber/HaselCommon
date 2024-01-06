using Dalamud.IoC;
using Dalamud.Plugin;

namespace HaselCommon;

internal class DalamudServiceWrapper<T>
{
    [PluginService] internal T Service { get; private set; } = default!;

    internal DalamudServiceWrapper(DalamudPluginInterface pi)
    {
        pi.Inject(this);
    }
}
