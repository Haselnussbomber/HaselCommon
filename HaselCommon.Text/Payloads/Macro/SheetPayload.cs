using Lumina.Text;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sheet)] // s . . .
public class SheetPayload : HaselMacroPayload
{
    public ExpressionWrapper? SheetName { get; set; }
    public ExpressionWrapper? RowId { get; set; }
    public ExpressionWrapper? ColumnIndex { get; set; }
    public ExpressionWrapper? ColumnParam { get; set; } // like lnum1 in the switch in GrandCompany.Unknown1

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (SheetName == null || RowId == null || ColumnIndex == null)
            return new();

        var sheet = Service.DataManager.Excel.GetSheetRaw(SheetName.ResolveString(localParameters).ToString());
        if (sheet == null)
            return new();

        var row = sheet.GetRow((uint)RowId.ResolveNumber(localParameters));
        if (row == null)
            return new();

        var text = row.ReadColumn<SeString>(ColumnIndex.ResolveNumber(localParameters));
        if (text == null)
            return new();

        return ColumnParam == null
            ? text.Resolve()
            : text.Resolve([ColumnParam]);
    }
}
