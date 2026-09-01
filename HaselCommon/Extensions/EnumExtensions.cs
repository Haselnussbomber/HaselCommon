using System.Runtime.CompilerServices;

namespace HaselCommon.Extensions;

public static class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast<T>(this T value, T flag) where T : struct, Enum
    {
        return Unsafe.SizeOf<T>() switch
        {
            1 => (Unsafe.As<T, byte>(ref value) & Unsafe.As<T, byte>(ref flag)) == Unsafe.As<T, byte>(ref flag),
            2 => (Unsafe.As<T, ushort>(ref value) & Unsafe.As<T, ushort>(ref flag)) == Unsafe.As<T, ushort>(ref flag),
            4 => (Unsafe.As<T, uint>(ref value) & Unsafe.As<T, uint>(ref flag)) == Unsafe.As<T, uint>(ref flag),
            8 => (Unsafe.As<T, ulong>(ref value) & Unsafe.As<T, ulong>(ref flag)) == Unsafe.As<T, ulong>(ref flag),
            _ => throw new NotSupportedException($"Unsupported enum size: {Unsafe.SizeOf<T>()} bytes.")
        };
    }

    public static unsafe void SetFlag<T>(this ref T flags, T flag, bool enable = true) where T : unmanaged, Enum
    {
        switch (sizeof(T))
        {
            case 1: flags.SetFlag<T, byte>(flag, enable); break;
            case 2: flags.SetFlag<T, ushort>(flag, enable); break;
            case 4: flags.SetFlag<T, uint>(flag, enable); break;
            case 8: flags.SetFlag<T, ulong>(flag, enable); break;
            default: throw new NotSupportedException("Unsupported enum size");
        }
    }

    public static void SetFlag<TEnum, TUnderlying>(this ref TEnum flags, TEnum flag, bool enable = true)
        where TEnum : unmanaged, Enum
        where TUnderlying : unmanaged, IBinaryInteger<TUnderlying>
    {
        ref var value = ref Unsafe.As<TEnum, TUnderlying>(ref flags);
        var mask = Unsafe.As<TEnum, TUnderlying>(ref flag);

        if (enable)
            value |= mask;
        else
            value &= ~mask;
    }
}
