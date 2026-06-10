namespace HaselCommon.Extensions;

public static unsafe class EquipSlotCategoryExtensions
{
    extension(EquipSlotCategory row)
    {
        public Collection<sbyte> Slots
            => new(row.ExcelPage, parentOffset: row.RowOffset, offset: row.RowOffset, &SlotCtor, size: row.ExcelPage.Sheet.Columns.Count);
    }

    private static sbyte SlotCtor(ExcelPage page, uint parentOffset, uint offset, uint i)
        => page.ReadInt8(offset + i);
}
