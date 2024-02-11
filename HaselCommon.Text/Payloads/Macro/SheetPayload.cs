using System.Collections.Generic;
using System.IO;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Sheet)] // s . . .
public class SheetPayload : HaselMacroPayload
{
    private static readonly IntegerExpression DefaultRowId = new(0);
    private static readonly IntegerExpression DefaultColumnIndex = new(0);

    public BaseExpression? SheetName { get; set; }
    public BaseExpression? RowId { get; set; } = DefaultRowId;
    public BaseExpression? ColumnIndex { get; set; } = DefaultColumnIndex;

    public override byte[] Encode() => EncodeChunk(SheetName);

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        SheetName = BaseExpression.Parse(reader.BaseStream);
        RowId = BaseExpression.Parse(reader.BaseStream);
        ColumnIndex = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (SheetName == null)
            return new();

        var sheet = Service.DataManager.Excel.GetSheetRaw(SheetName.ResolveString(localParameterData).ToString());
        if (sheet == null)
            return new();

        var rowId = RowId ?? DefaultRowId;
        var columnIndex = ColumnIndex ?? DefaultColumnIndex;

        var row = sheet.GetRow((uint)rowId.ResolveNumber(localParameterData));
        if (row == null)
            return new();

        var text = row.ReadColumn<Lumina.Text.SeString>(columnIndex.ResolveNumber(localParameterData));
        if (text == null)
            return new();

        return text.Resolve(localParameterData);
    }
}
