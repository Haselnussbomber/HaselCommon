namespace HaselCommon.Structs.Internal;

[StructLayout(LayoutKind.Explicit, Size = 0x100)] // probably size of item row?
internal struct ItemActionUnlockedData
{
    [FieldOffset(0x70)] public uint ItemActionAdditionalData;
    [FieldOffset(0x8E)] public uint ItemActionId;
}
