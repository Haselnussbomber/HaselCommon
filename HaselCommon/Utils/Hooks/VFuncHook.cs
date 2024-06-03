using Dalamud.Hooking;

namespace HaselCommon.Utils;

public sealed unsafe class VFuncHook<T>(nint vtblAddress, int vfIndex, T detour) : IHook<T> where T : Delegate
{
    public VFuncHook(void* vtblAddress, int vfIndex, T detour) : this((nint)vtblAddress, vfIndex, detour)
    {
    }

    public Hook<T> Hook { get; } = Service.GameInteropProvider.HookFromFunctionPointerVariable(vtblAddress + vfIndex * 0x08, detour);

    public void Enable() => Hook.Enable();
    public void Disable() => Hook.Disable();
    void IDisposable.Dispose() => Hook.Dispose();

    public bool IsEnabled => Hook.IsEnabled;
    public T Original => Hook.Original;
    public T OriginalDisposeSafe => Hook.OriginalDisposeSafe;
}
