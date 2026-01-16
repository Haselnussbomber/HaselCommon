namespace HaselCommon.Extensions;

public static unsafe class EquipRaceCategoryExtensions
{
    extension(EquipRaceCategory row)
    {
        public Collection<bool> Races
            => new(row.ExcelPage, parentOffset: row.RowOffset, offset: row.RowOffset, &RaceCtor, size: row.ExcelPage.Module.GetSheet<Race>().Count);
    }

    private static bool RaceCtor(ExcelPage page, uint parentOffset, uint offset, uint i)
        => page.ReadBool(offset + i);
}
