namespace HaselCommon.Extensions;

public static class IGameInteropProviderExtensions
{
    public static unsafe Hook<T> HookFromVTable<T>(this IGameInteropProvider gameInteropProvider, void* vtblAddress, int vfIndex, T detour) where T : Delegate
        => gameInteropProvider.HookFromVTable((nint)vtblAddress, vfIndex, detour);

    public static unsafe Hook<T> HookFromVTable<T>(this IGameInteropProvider gameInteropProvider, nint vtblAddress, int vfIndex, T detour) where T : Delegate
        => gameInteropProvider.HookFromAddress(*(nint*)(vtblAddress + vfIndex * 0x08), detour);
}
