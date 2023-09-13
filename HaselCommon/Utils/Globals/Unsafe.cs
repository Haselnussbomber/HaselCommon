using System.Runtime.CompilerServices;

namespace HaselCommon.Utils.Globals;

public static unsafe class UnsafeGlobals
{
    public static T* AsPointer<T>(ref T entryRef) where T : unmanaged
        => (T*)Unsafe.AsPointer(ref entryRef);
}
