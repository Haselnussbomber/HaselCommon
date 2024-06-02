using Dalamud.Hooking;

namespace HaselCommon.Utils;

public interface IHook<T> : IDisposable where T : Delegate
{
    Hook<T> Hook { get; }
    void Enable();
    void Disable();
    bool IsEnabled { get; }
    T OriginalDisposeSafe { get; }
}
