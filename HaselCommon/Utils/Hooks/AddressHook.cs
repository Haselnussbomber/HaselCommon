using Dalamud.Hooking;
using HaselCommon.Interfaces;

namespace HaselCommon.Utils;

public sealed unsafe class AddressHook<T>(nint address, T detour) : IHook<T> where T : Delegate
{
    public AddressHook(void* address, T detour) : this((nint)address, detour)
    {
    }

    public Hook<T> Hook { get; } = Service.GameInteropProvider.HookFromAddress(address, detour);

    public void Enable() => Hook.Enable();
    public void Disable() => Hook.Disable();
    void IDisposable.Dispose() => Hook.Dispose();

    public bool IsEnabled => Hook.IsEnabled;
    public T Original => Hook.Original;
    public T OriginalDisposeSafe => Hook.OriginalDisposeSafe;
}
