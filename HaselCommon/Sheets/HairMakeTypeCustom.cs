using System.Collections.Generic;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HaselCommon.Sheets;

[Sheet("HairMakeType")]
public readonly struct HairMakeTypeCustom(ExcelPage page, uint offset, uint row) : IExcelRow<HairMakeTypeCustom>
{
    public uint RowId => row;

    // public RowRef<HairMakeType> HairMakeType => new(page.Module, row, page.Language);
    public HairMakeType HairMakeType => page.Module.GetSheet<HairMakeType>().GetRow(RowId);

    public RowRef<CharaMakeCustomize>[] HairStyles
    {
        get
        {
            var hairstyles = new List<RowRef<CharaMakeCustomize>>();

            for (var i = 0u; i < 200; i++)
            {
                var id = page.ReadUInt32(offset + 0xC + 4 * i);
                if (id == 0) break;
                hairstyles.Add(new RowRef<CharaMakeCustomize>(page.Module, id));
            }

            return [.. hairstyles];
        }
    }

    static HairMakeTypeCustom IExcelRow<HairMakeTypeCustom>.Create(ExcelPage page, uint offset, uint row)
        => new(page, offset, row);
}
