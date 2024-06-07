using Dalamud.Hooking;

namespace HaselCommon.Interfaces;

public interface IHook<T> : IDisposable where T : Delegate
{
    Hook<T> Hook { get; }
    void Enable();
    void Disable();
    bool IsEnabled { get; }
    T Original { get; }
    T OriginalDisposeSafe { get; }
}
