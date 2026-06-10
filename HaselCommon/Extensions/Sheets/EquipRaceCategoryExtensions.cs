namespace HaselCommon.Extensions;

public static unsafe class EquipRaceCategoryExtensions
{
    extension(EquipRaceCategory row)
    {
        public Collection<bool> Races
            => new(row.ExcelPage, parentOffset: row.RowOffset, offset: row.RowOffset, &RaceCtor, size: row.ExcelPage.Sheet.Columns.Count - 2);
    }

    private static bool RaceCtor(ExcelPage page, uint parentOffset, uint offset, uint i)
        => page.ReadBool(offset + i);
}
