namespace HaselCommon.Extensions;

public static partial class IGameInteropProviderExtensions
{
    extension(IGameInteropProvider gameInteropProvider)
    {
        public Hook<TDelegate> EnabledHookFromAddress<TDelegate>(nint address, TDelegate detour) where TDelegate : Delegate
        {
            var hook = gameInteropProvider.HookFromAddress(address, detour);
            hook.Enable();
            return hook;
        }

        public unsafe Hook<TDelegate> EnabledHookFromAddress<TDelegate>(void* address, TDelegate detour) where TDelegate : Delegate
        {
            var hook = gameInteropProvider.HookFromAddress((nint)address, detour);
            hook.Enable();
            return hook;
        }

        public Hook<TDelegate> EnabledHookFromSignature<TDelegate>(string signature, TDelegate detour) where TDelegate : Delegate
        {
            var hook = gameInteropProvider.HookFromSignature(signature, detour);
            hook.Enable();
            return hook;
        }

        public unsafe Hook<TDelegate> EnabledHookFromVTable<TDelegate>(void* vtblAddress, int vfIndex, TDelegate detour) where TDelegate : Delegate
        {
            return gameInteropProvider.EnabledHookFromVTable((nint)vtblAddress, vfIndex, detour);
        }

        public unsafe Hook<TDelegate> EnabledHookFromVTable<TDelegate>(nint vtblAddress, int vfIndex, TDelegate detour) where TDelegate : Delegate
        {
            return gameInteropProvider.EnabledHookFromAddress(*(nint*)(vtblAddress + vfIndex * 0x08), detour);
        }
    }
}
