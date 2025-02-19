using System.Diagnostics.CodeAnalysis;
using HaselCommon.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HaselCommon.Utils;

public readonly struct ExcelRowId<T>(uint rowId) where T : struct, IExcelRow<T>
{
    public readonly uint RowId { get; } = rowId;

    public bool TryGetRow(out T row)
    {
        if (!Service.TryGet<ExcelService>(out var excelService))
        {
            row = default;
            return false;
        }

        return excelService.TryGetRow(RowId, out row);
    }

    public bool TryGetRow<TRow>(out TRow row) where TRow : struct, IExcelRow<TRow>
    {
        if (!Service.TryGet<ExcelService>(out var excelService))
        {
            row = default;
            return false;
        }

        return excelService.TryGetRow(RowId, out row);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ExcelRowId<T> rowId && RowId == rowId;
    public override int GetHashCode() => RowId.GetHashCode();
    public override string ToString() => RowId.ToString();

    public static bool operator ==(ExcelRowId<T> left, ExcelRowId<T> right) => left.RowId == right.RowId;
    public static bool operator !=(ExcelRowId<T> left, ExcelRowId<T> right) => left.RowId != right.RowId;

    public static implicit operator ExcelRowId<T>(T row) => new(row.RowId);
    public static implicit operator ExcelRowId<T>(RowRef<T> rowRef) => new(rowRef.RowId);
    public static implicit operator ExcelRowId<T>(uint rowId) => new(rowId);

    public static implicit operator uint(ExcelRowId<T> rowId) => rowId.RowId;
}

// ExcelRowId<Item> extensions
public static class ExcelRowIdExtensions
{
    private static int? EventItemRowCount;

    public static ExcelRowId<Item> GetBaseId(this ExcelRowId<Item> id)
    {
        if (id.IsEventItem()) return id.RowId; // uses EventItem sheet
        if (id.IsHighQuality()) return id.RowId - 1_000_000;
        if (id.IsCollectible()) return id.RowId - 500_000;
        return id.RowId;
    }

    public static bool IsNormalItem(this ExcelRowId<Item> itemId)
    {
        return itemId.RowId < 500_000;
    }

    public static bool IsCollectible(this ExcelRowId<Item> itemId)
    {
        return itemId.RowId is >= 500_000 and < 1_000_000;
    }

    public static bool IsHighQuality(this ExcelRowId<Item> itemId)
    {
        return itemId.RowId is >= 1_000_000 and < 2_000_000;
    }

    public static bool IsEventItem(this ExcelRowId<Item> itemId)
    {
        return itemId.RowId >= 2_000_000 && itemId.RowId - 2_000_000 < (EventItemRowCount ??= Service.Get<ExcelService>().GetRowCount<EventItem>());
    }
}
