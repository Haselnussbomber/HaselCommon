using Lumina.Text;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sheet)] // s . . .
public class SheetPayload : HaselMacroPayload
{
    public BaseExpression? SheetName { get; set; }
    public BaseExpression? RowId { get; set; }
    public BaseExpression? ColumnIndex { get; set; }

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

        return text.Resolve(localParameters);
    }
}
