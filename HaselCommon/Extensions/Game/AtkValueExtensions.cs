using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace HaselCommon.Extensions;

// not implemented: Vector, AtkValues

public static class AtkValueExtensions
{
    extension(ref AtkValue atkValue)
    {
        public bool Is(ValueType type)
        {
            return (atkValue.Type & ValueType.TypeMask) == type;
        }

        public bool TryGetBool(out bool value)
        {
            var isType = atkValue.Is(ValueType.Bool);
            value = isType && atkValue.Bool;
            return isType;
        }

        public bool TryGetInt(out int value)
        {
            var isType = atkValue.Is(ValueType.Int);
            value = isType ? atkValue.Int : default;
            return isType;
        }

        public bool TryGetInt64(out long value)
        {
            var isType = atkValue.Is(ValueType.Int64);
            value = isType ? atkValue.Int64 : default;
            return isType;
        }

        public bool TryGetUInt(out uint value)
        {
            var isType = atkValue.Is(ValueType.UInt);
            value = isType ? atkValue.UInt : default;
            return isType;
        }

        public bool TryGetUInt64(out ulong value)
        {
            var isType = atkValue.Is(ValueType.UInt64);
            value = isType ? atkValue.UInt64 : default;
            return isType;
        }

        public bool TryGetFloat(out float value)
        {
            var isType = atkValue.Is(ValueType.Float);
            value = isType ? atkValue.Float : default;
            return isType;
        }

        public unsafe bool TryGetString(out ReadOnlySeStringSpan value)
        {
            var isType = atkValue.Is(ValueType.String) || atkValue.Is(ValueType.String8);
            value = isType ? new ReadOnlySeStringSpan(atkValue.String.Value) : default;
            return isType;
        }

        public unsafe bool TryGetWideString(out string value)
        {
            var isType = atkValue.Is(ValueType.WideString);
            value = isType ? new string(atkValue.WideString) : string.Empty;
            return isType;
        }

        public unsafe bool TryGetPointer(out nint value)
        {
            var isType = atkValue.Is(ValueType.Pointer);
            value = isType ? (nint)atkValue.Pointer : default;
            return isType;
        }

        public unsafe bool TryGetPointer<T>(out T* value) where T : unmanaged
        {
            var isType = atkValue.Is(ValueType.Pointer);
            value = isType ? (T*)atkValue.Pointer : default;
            return isType;
        }
    }
}
