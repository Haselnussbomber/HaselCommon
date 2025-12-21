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
    public readonly Collection<bool> ClassesJobs => new(page, offset, offset, &ClassesJobsCtor, 46);
    public readonly bool ADV => page.ReadBool(offset + 4);
    public readonly bool GLA => page.ReadBool(offset + 5);
    public readonly bool PGL => page.ReadBool(offset + 6);
    public readonly bool MRD => page.ReadBool(offset + 7);
    public readonly bool LNC => page.ReadBool(offset + 8);
    public readonly bool ARC => page.ReadBool(offset + 9);
    public readonly bool CNJ => page.ReadBool(offset + 10);
    public readonly bool THM => page.ReadBool(offset + 11);
    public readonly bool CRP => page.ReadBool(offset + 12);
    public readonly bool BSM => page.ReadBool(offset + 13);
    public readonly bool ARM => page.ReadBool(offset + 14);
    public readonly bool GSM => page.ReadBool(offset + 15);
    public readonly bool LTW => page.ReadBool(offset + 16);
    public readonly bool WVR => page.ReadBool(offset + 17);
    public readonly bool ALC => page.ReadBool(offset + 18);
    public readonly bool CUL => page.ReadBool(offset + 19);
    public readonly bool MIN => page.ReadBool(offset + 20);
    public readonly bool BTN => page.ReadBool(offset + 21);
    public readonly bool FSH => page.ReadBool(offset + 22);
    public readonly bool PLD => page.ReadBool(offset + 23);
    public readonly bool MNK => page.ReadBool(offset + 24);
    public readonly bool WAR => page.ReadBool(offset + 25);
    public readonly bool DRG => page.ReadBool(offset + 26);
    public readonly bool BRD => page.ReadBool(offset + 27);
    public readonly bool WHM => page.ReadBool(offset + 28);
    public readonly bool BLM => page.ReadBool(offset + 29);
    public readonly bool ACN => page.ReadBool(offset + 30);
    public readonly bool SMN => page.ReadBool(offset + 31);
    public readonly bool SCH => page.ReadBool(offset + 32);
    public readonly bool ROG => page.ReadBool(offset + 33);
    public readonly bool NIN => page.ReadBool(offset + 34);
    public readonly bool MCH => page.ReadBool(offset + 35);
    public readonly bool DRK => page.ReadBool(offset + 36);
    public readonly bool AST => page.ReadBool(offset + 37);
    public readonly bool SAM => page.ReadBool(offset + 38);
    public readonly bool RDM => page.ReadBool(offset + 39);
    public readonly bool BLU => page.ReadBool(offset + 40);
    public readonly bool GNB => page.ReadBool(offset + 41);
    public readonly bool DNC => page.ReadBool(offset + 42);
    public readonly bool RPR => page.ReadBool(offset + 43);
    public readonly bool SGE => page.ReadBool(offset + 44);
    public readonly bool VPR => page.ReadBool(offset + 45);
    public readonly bool PCT => page.ReadBool(offset + 46);
    public readonly bool Unknown0 => page.ReadBool(offset + 47);
    public readonly bool Unknown1 => page.ReadBool(offset + 48);
    public readonly bool Unknown2 => page.ReadBool(offset + 49);

    private static bool ClassesJobsCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadBool(offset + 4 + i);

    static CustomClassJobCategory IExcelRow<CustomClassJobCategory>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
