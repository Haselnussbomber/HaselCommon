using Dalamud.IoC;
using Dalamud.Plugin;

namespace HaselCommon.Services.Internal;

internal class DalamudServiceWrapper<T>
{
    [PluginService] internal T Service { get; private set; } = default!;

    internal DalamudServiceWrapper(IDalamudPluginInterface pi)
    {
        pi.Inject(this);
    }
}
