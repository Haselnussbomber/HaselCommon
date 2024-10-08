using System.Collections.Generic;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class HairMakeTypeCustom : HairMakeType
{
    public LazyRow<CharaMakeCustomize>[] HairStyles { get; private set; } = [];

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        var hairstyles = new List<uint>();

        for (var i = 0; i < 200; i++)
        {
            var id = parser.ReadOffset<uint>(0xC + 4 * i);
            if (id == 0) break;
            hairstyles.Add(id);
        }

        HairStyles = new LazyRow<CharaMakeCustomize>[hairstyles.Count];
        for (var i = 0; i < hairstyles.Count; i++)
        {
            HairStyles[i] = new LazyRow<CharaMakeCustomize>(gameData, hairstyles[i], language);
        }
    }
}
