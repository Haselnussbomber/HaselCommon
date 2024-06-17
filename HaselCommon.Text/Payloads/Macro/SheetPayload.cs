using Dalamud.Plugin.Services;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sheet)] // s . . .
public class SheetPayload : MacroPayload
{
    public Expression? SheetName { get; set; }
    public Expression? RowId { get; set; }
    public Expression? ColumnIndex { get; set; }
    public Expression? ColumnParam { get; set; } // like lnum1 in the switch in GrandCompany.Unknown1

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (SheetName == null || RowId == null || ColumnIndex == null)
            return new();

        var sheet = Service.Get<IDataManager>().Excel.GetSheetRaw(SheetName.ResolveString(localParameters).ToString());
        if (sheet == null)
            return new();

        var row = sheet.GetRow((uint)RowId.ResolveNumber(localParameters));
        if (row == null)
            return new();

        var text = row.ReadColumn<Lumina.Text.SeString>(ColumnIndex.ResolveNumber(localParameters));
        if (text == null)
            return new();

        if (ColumnParam == null)
            return text.Resolve();

        // HACK: (I think) we need to subtract 1, because passing it to Resolve is casting the int to ExpressionWrapper, which creates an IntegerExpression that adds 1 during encode
        var columnParam = (ColumnParam?.ResolveNumber(localParameters) ?? 1) - 1;
        return text.Resolve([columnParam]);
    }
}
