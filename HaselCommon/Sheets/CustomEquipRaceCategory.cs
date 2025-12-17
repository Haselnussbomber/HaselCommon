namespace HaselCommon.Sheets;

/// <summary>
/// <see cref="EquipRaceCategory"/>
/// </summary>
[Sheet("EquipRaceCategory", 0xF914B198)]
public readonly unsafe struct CustomEquipRaceCategory(ExcelPage page, uint offset, uint row) : IExcelRow<CustomEquipRaceCategory>
{
    public ExcelPage ExcelPage => page;
    public uint RowOffset => offset;
    public uint RowId => row;

    public readonly Collection<bool> Races => new(page, offset, offset, &RacesCtor, 8);
    public readonly bool Hyur => page.ReadBool(offset);
    public readonly bool Elezen => page.ReadBool(offset + 1);
    public readonly bool Lalafell => page.ReadBool(offset + 2);
    public readonly bool Miqote => page.ReadBool(offset + 3);
    public readonly bool Roegadyn => page.ReadBool(offset + 4);
    public readonly bool AuRa => page.ReadBool(offset + 5);
    public readonly bool Hrothgar => page.ReadBool(offset + 6);
    public readonly bool Viera => page.ReadBool(offset + 7);
    public readonly bool Male => page.ReadPackedBool(offset + 8, 0);
    public readonly bool Female => page.ReadPackedBool(offset + 8, 1);

    private static bool RacesCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadBool(offset + i);

    static CustomEquipRaceCategory IExcelRow<CustomEquipRaceCategory>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
