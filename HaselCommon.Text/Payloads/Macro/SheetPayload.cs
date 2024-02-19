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

        if (ColumnParam == null)
            return text.Resolve();

        // HACK: (I think) we need to subtract 1, because passing it to Resolve is casting the int to ExpressionWrapper, which creates an IntegerExpression that adds 1 during encode
        var columnParam = (ColumnParam?.ResolveNumber(localParameters) ?? 1) - 1;
        return text.Resolve([columnParam]);
    }
}
