using System.Diagnostics.CodeAnalysis;
using HaselCommon.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HaselCommon.Utils;

public readonly struct ItemId(uint rowId)
{
    private static int? EventItemRowCount;

    public readonly uint RowId = rowId;

    public ItemId BaseItemId
    {
        get
        {
            if (IsEventItem) return RowId; // uses EventItem sheet
            if (IsHighQuality) return RowId - 1_000_000;
            if (IsCollectible) return RowId - 500_000;
            return RowId;
        }
    }

    public bool IsNormalItem => RowId < 500_000;
    public bool IsCollectible => RowId is >= 500_000 and < 1_000_000;
    public bool IsHighQuality => RowId is >= 1_000_000 and < 2_000_000;
    public bool IsEventItem => RowId >= 2_000_000 && RowId - 2_000_000 < (EventItemRowCount ??= Service.Get<ExcelService>().GetRowCount<EventItem>());

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is uint rowId && RowId == rowId;
    public override int GetHashCode() => RowId.GetHashCode();
    public override string ToString() => RowId.ToString();

    public static implicit operator ItemId(Item row) => new(row.RowId);
    public static implicit operator ItemId(RowRef<Item> rowRef) => new(rowRef.RowId);
    public static implicit operator ItemId(uint rowId) => new(rowId);

    public static implicit operator uint(ItemId itemRef) => itemRef.RowId;
}
