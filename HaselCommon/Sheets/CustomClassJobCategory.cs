namespace HaselCommon.Sheets;

/// <summary>
/// <see cref="ClassJobCategory"/>
/// </summary>
[Sheet("ClassJobCategory", 0x6733E334)]
public readonly unsafe struct CustomClassJobCategory(ExcelPage page, uint offset, uint row) : IExcelRow<CustomClassJobCategory>
{
    public ExcelPage ExcelPage => page;
    public uint RowOffset => offset;
    public uint RowId => row;

    public readonly ReadOnlySeString Name => page.ReadString(offset, offset);
    public readonly Collection<bool> ClassesJobs => new(page, offset, offset, &ClassesJobsCtor, 8);

    private static bool ClassesJobsCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadBool(offset + 4 + i);

    static CustomClassJobCategory IExcelRow<CustomClassJobCategory>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
