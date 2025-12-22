namespace HaselCommon.Extensions;

public static class ClassJobExtensions
{
    extension(ClassJob row)
    {
        public bool IsGatherer => row.ClassJobCategory.RowId == 32;
        public bool IsCrafter => row.ClassJobCategory.RowId == 33;
    }
}
