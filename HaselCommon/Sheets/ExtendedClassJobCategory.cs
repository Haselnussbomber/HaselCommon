using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedClassJobCategory : ClassJobCategory
{
    private const int NumClasses = 43;

    public bool[] ClassJobs { get; set; } = Array.Empty<bool>();

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        ClassJobs = new bool[NumClasses];
        for (var i = 0; i < NumClasses; i++)
        {
            ClassJobs[i] = parser.ReadColumn<bool>(i + 1);
        }
    }
}
