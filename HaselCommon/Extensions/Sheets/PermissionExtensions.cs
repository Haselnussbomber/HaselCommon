namespace HaselCommon.Extensions;

public static unsafe class PermissionExtensions
{
    extension(Permission row)
    {
        public Collection<bool> Conditions
            => new(row.ExcelPage, row.RowOffset, row.RowOffset, &ConditionCtor, row.ExcelPage.Sheet.Columns.Count);
    }

    private static bool ConditionCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadBool(offset + i);
}
