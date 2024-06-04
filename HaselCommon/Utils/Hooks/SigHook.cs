using Dalamud.Hooking;

namespace HaselCommon.Utils;

public sealed unsafe class SigHook<T>(string signature, T detour) : IHook<T> where T : Delegate
{
    public Hook<T> Hook { get; } = Service.GameInteropProvider.HookFromSignature(signature, detour);

    public void Enable() => Hook.Enable();
    public void Disable() => Hook.Disable();
    void IDisposable.Dispose() => Hook.Dispose();

    public bool IsEnabled => Hook.IsEnabled;
    public T Original => Hook.Original;
    public T OriginalDisposeSafe => Hook.OriginalDisposeSafe;
}
