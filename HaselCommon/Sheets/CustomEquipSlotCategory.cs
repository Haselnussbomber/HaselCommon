namespace HaselCommon.Sheets;

/// <summary>
/// <see cref="EquipSlotCategory"/>
/// </summary>
[Sheet("EquipSlotCategory", 0xF4AB6454)]
public readonly unsafe struct CustomEquipSlotCategory(ExcelPage page, uint offset, uint row) : IExcelRow<EquipSlotCategory>
{
    public ExcelPage ExcelPage => page;
    public uint RowOffset => offset;
    public uint RowId => row;

    public readonly Collection<sbyte> Slots => new(page, parentOffset: offset, offset: offset, &SlotCtor, size: 14);
    public readonly sbyte MainHand => page.ReadInt8(offset);
    public readonly sbyte OffHand => page.ReadInt8(offset + 1);
    public readonly sbyte Head => page.ReadInt8(offset + 2);
    public readonly sbyte Body => page.ReadInt8(offset + 3);
    public readonly sbyte Gloves => page.ReadInt8(offset + 4);
    public readonly sbyte Waist => page.ReadInt8(offset + 5);
    public readonly sbyte Legs => page.ReadInt8(offset + 6);
    public readonly sbyte Feet => page.ReadInt8(offset + 7);
    public readonly sbyte Ears => page.ReadInt8(offset + 8);
    public readonly sbyte Neck => page.ReadInt8(offset + 9);
    public readonly sbyte Wrists => page.ReadInt8(offset + 10);
    public readonly sbyte FingerL => page.ReadInt8(offset + 11);
    public readonly sbyte FingerR => page.ReadInt8(offset + 12);
    public readonly sbyte SoulCrystal => page.ReadInt8(offset + 13);

    private static sbyte SlotCtor(ExcelPage page, uint parentOffset, uint offset, uint i)
        => page.ReadInt8(offset + i);

    static EquipSlotCategory IExcelRow<EquipSlotCategory>.Create(ExcelPage page, uint offset, uint row)
        => new(page, offset, row);
}
