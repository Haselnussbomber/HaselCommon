using System.Runtime.CompilerServices;

namespace HaselCommon.Utils.Globals;

public static unsafe class UnsafeGlobals
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* AsPointer<T>(ref T entryRef) where T : unmanaged
        => (T*)Unsafe.AsPointer(ref entryRef);
}
