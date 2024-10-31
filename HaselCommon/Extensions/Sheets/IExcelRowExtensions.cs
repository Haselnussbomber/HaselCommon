using Dalamud.Plugin.Services;
using Lumina.Excel;

namespace HaselCommon.Extensions.Sheets;

public static class IExcelRowExtensions
{
    public static RowRef<T> AsRef<T>(this IExcelRow<T> row) where T : struct, IExcelRow<T>
        => new(Service.Get<IDataManager>().Excel, row.RowId);
}
