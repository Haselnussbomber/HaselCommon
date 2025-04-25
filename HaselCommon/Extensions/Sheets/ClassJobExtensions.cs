using Lumina.Excel.Sheets;

namespace HaselCommon.Extensions.Sheets;

public static class ClassJobExtensions
{
    public static bool IsGatherer(this ClassJob row) => row.ClassJobCategory.RowId == 32;
    public static bool IsCrafter(this ClassJob row) => row.ClassJobCategory.RowId == 33;
}
