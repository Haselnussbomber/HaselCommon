namespace HaselCommon.Extensions;

public static class RowRefExtensions
{
    extension<T>(RowRef<T> rowRef) where T : struct, IExcelRow<T>
    {
        public bool TryGetRow(out T row)
        {
            if (rowRef.IsValid)
            {
                row = rowRef.Value;
                return true;
            }

            row = default;
            return false;
        }
    }
}
