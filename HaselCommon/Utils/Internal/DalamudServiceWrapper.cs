using Dalamud.IoC;

namespace HaselCommon.Utils.Internal;

//! https://github.com/Ottermandias/OtterGui/blob/3bf047b/Services/ServiceManager.cs#L133
internal class DalamudServiceWrapper<T>
{
    [PluginService] internal T Service { get; private set; } = default!;

    internal DalamudServiceWrapper(IDalamudPluginInterface pi)
    {
        pi.Inject(this);
    }
}
