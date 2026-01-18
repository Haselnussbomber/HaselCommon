using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.STD;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace HaselCommon.Extensions;

public unsafe static class AtkValueExtensions
{
    extension(ref AtkValue atkValue)
    {
        public bool IsUndefined => atkValue.Is(ValueType.Undefined);
        public bool IsNull => atkValue.Is(ValueType.Null);
        public bool IsBool => atkValue.Is(ValueType.Bool);
        public bool IsInt => atkValue.Is(ValueType.Int);
        public bool IsInt64 => atkValue.Is(ValueType.Int64);
        public bool IsUInt => atkValue.Is(ValueType.UInt);
        public bool IsUInt64 => atkValue.Is(ValueType.UInt64);
        public bool IsFloat => atkValue.Is(ValueType.Float);
        public bool IsString => atkValue.Is(ValueType.String) || atkValue.Is(ValueType.String8);
        public bool IsWideString => atkValue.Is(ValueType.WideString);
        public bool IsVector => atkValue.Is(ValueType.Vector);
        public bool IsPointer => atkValue.Is(ValueType.Pointer);
        public bool IsAtkValues => atkValue.Is(ValueType.AtkValues);

        public bool Is(ValueType type) => (atkValue.Type & ValueType.TypeMask) == type;

        public bool TryGetBool(out bool value)
        {
            var isType = atkValue.IsBool;
            value = isType && atkValue.Bool;
            return isType;
        }

        public bool TryGetInt(out int value)
        {
            var isType = atkValue.IsInt;
            value = isType ? atkValue.Int : default;
            return isType;
        }

        public bool TryGetInt64(out long value)
        {
            var isType = atkValue.IsInt64;
            value = isType ? atkValue.Int64 : default;
            return isType;
        }

        public bool TryGetUInt(out uint value)
        {
            var isType = atkValue.IsUInt;
            value = isType ? atkValue.UInt : default;
            return isType;
        }

        public bool TryGetUInt64(out ulong value)
        {
            var isType = atkValue.IsUInt64;
            value = isType ? atkValue.UInt64 : default;
            return isType;
        }

        public bool TryGetFloat(out float value)
        {
            var isType = atkValue.IsFloat;
            value = isType ? atkValue.Float : default;
            return isType;
        }

        public bool TryGetString(out ReadOnlySeStringSpan value)
        {
            var isType = atkValue.IsString;
            value = isType ? new ReadOnlySeStringSpan(atkValue.String.Value) : default;
            return isType;
        }

        public bool TryGetWideString(out string value)
        {
            var isType = atkValue.IsWideString;
            value = isType ? new string(atkValue.WideString) : string.Empty;
            return isType;
        }

        public bool TryGetVector(out StdVector<AtkValue> value)
        {
            var isType = atkValue.IsVector;
            value = isType && atkValue.Vector != null ? *atkValue.Vector : default;
            return isType;
        }

        public bool TryGetPointer(out nint value)
        {
            var isType = atkValue.IsPointer;
            value = isType ? (nint)atkValue.Pointer : default;
            return isType;
        }

        public bool TryGetPointer<T>(out T* value) where T : unmanaged
        {
            var isType = atkValue.IsPointer;
            value = isType ? (T*)atkValue.Pointer : default;
            return isType;
        }

        public bool TryGetAtkValues(out AtkValue* value)
        {
            var isType = atkValue.IsAtkValues;
            value = isType ? atkValue.AtkValues : default;
            return isType;
        }

        public bool TryGetAtkValues(int valueCount, out Span<AtkValue> value)
        {
            var isType = atkValue.IsAtkValues;
            value = isType ? new(atkValue.AtkValues, valueCount) : default;
            return isType;
        }
    }
}
