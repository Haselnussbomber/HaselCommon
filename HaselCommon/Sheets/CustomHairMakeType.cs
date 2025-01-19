using Lumina.Excel;
using Lumina.Excel.Sheets;

using CharaMakeStructStruct = Lumina.Excel.Sheets.CharaMakeType.CharaMakeStructStruct;
using FacialFeatureOptionStruct = Lumina.Excel.Sheets.CharaMakeType.FacialFeatureOptionStruct;

namespace HaselCommon.Sheets;

// TODO: remove when this lands in Dalamud :)
// https://github.com/xivdev/EXDSchema/pull/61
[Sheet("HairMakeType")]
public readonly struct CustomHairMakeType(ExcelPage page, uint offset, uint row) : IExcelRow<CustomHairMakeType>
{
    public uint RowId => row;

    public unsafe Collection<CharaMakeStructStruct> CharaMakeStruct => new Collection<CharaMakeStructStruct>(page, offset, offset, (delegate*<ExcelPage, uint, uint, uint, CharaMakeStructStruct>)(&CharaMakeStructCtor), 9);
    public unsafe Collection<FacialFeatureOptionStruct> FacialFeatureOption => new Collection<FacialFeatureOptionStruct>(page, offset, offset, (delegate*<ExcelPage, uint, uint, uint, FacialFeatureOptionStruct>)(&FacialFeatureOptionCtor), 8);
    public RowRef<Race> Race => new RowRef<Race>(page.Module, (uint)page.ReadInt32(offset + 4076), page.Language);
    public RowRef<Tribe> Tribe => new RowRef<Tribe>(page.Module, (uint)page.ReadInt32(offset + 4080), page.Language);
    public sbyte Gender => page.ReadInt8(offset + 4084);

    private static CharaMakeStructStruct CharaMakeStructCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new CharaMakeStructStruct(page, parentOffset, offset + i * 428);
    private static FacialFeatureOptionStruct FacialFeatureOptionCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new FacialFeatureOptionStruct(page, parentOffset, offset + 3852 + i * 28);
    static CustomHairMakeType IExcelRow<CustomHairMakeType>.Create(ExcelPage page, uint offset, uint row) => new CustomHairMakeType(page, offset, row);
}
