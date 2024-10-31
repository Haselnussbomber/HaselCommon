using System.Collections.Generic;
using System.Text;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using HaselCommon.Extensions.Strings;
using Lumina.Excel;
using Lumina.Text;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

/*
Attributive sheet:
  Japanese:
    Unknown0 - Unknown1
  English:
    Unknown2 - Unknown7
  German:
    Unknown8 = Nominative Masculine
    Unknown9 = Nominative Feminine
    Unknown10 = Nominative Neutral
    Unknown11 = Nominative Plural
    Unknown12 = Genitive Masculine
    Unknown13 = Genitive Feminine
    Unknown14 = Genitive Neutral
    Unknown15 = Genitive Plural
    Unknown16 = Dative Masculine
    Unknown17 = Dative Feminine
    Unknown18 = Dative Neutral
    Unknown19 = Dative Plural
    Unknown20 = Accusative Masculine
    Unknown21 = Accusative Feminine
    Unknown22 = Accusative Neutral
    Unknown23 = Accusative Plural
  French:
    Unknown24 - Unknown40
*/

public class TextDecoder(ILogger<TextDecoder> logger, IDataManager dataManager)
{
    private readonly ReadOnlySeString _empty = new();
    private readonly Dictionary<(ClientLanguage Language, string SheetName, int RowId, int Amount, int Person, int Case), ReadOnlySeString> _cache = [];

    private const int SingularColumnIdx = 0;
    private const int AdjectiveColumnIdx = 1;
    private const int PluralColumnIdx = 2;
    private const int PossessivePronounColumnIdx = 3;
    private const int StartsWithVowelColumnIdx = 4;
    private const int Unknown5ColumnIdx = 5;
    private const int PronounColumnIdx = 6;
    private const int ArticleColumnIdx = 7;

    // <XXnoun(SheetName,Person,RowId,Amount,Case[,UnkInt5])>
    // UnkInt5 seems unused in En/Fr/De/Ja, so it's ignored for now
    public ReadOnlySeString ProcessNoun(ClientLanguage language, string sheetName, int person, int rowId, int amount = 1, int grammaticalCase = 1, int unkInt5 = 1)
    {
        grammaticalCase--;

        if (grammaticalCase > 5 || (language != ClientLanguage.German && grammaticalCase != 0))
            return _empty;

        var key = (language, sheetName, rowId, amount, person, grammaticalCase);
        if (_cache.TryGetValue(key, out var value))
            return value;

        var attributiveSheet = dataManager.GameData.Excel.GetSheet<RawRow>(language.ToLumina(), "Attributive");
        if (attributiveSheet == null)
        {
            logger.LogWarning("Sheet Attributive not found");
            return _empty;
        }

        var sheet = dataManager.GameData.Excel.GetSheet<RawRow>(language.ToLumina(), sheetName);
        if (sheet == null)
        {
            logger.LogWarning("Sheet {SheetName} not found", sheetName);
            return _empty;
        }

        if (!sheet.HasRow((uint)rowId))
        {
            logger.LogWarning("Sheet {SheetName} does not contain row #{RowId}", sheetName, rowId);
            return _empty;
        }

        var row = sheet.GetRow((uint)rowId);

        // see "E8 ?? ?? ?? ?? 44 8B 6B 08"
        var columnOffset = sheetName switch
        {
            "BeastTribe" => 10,
            "DeepDungeonItem" or "DeepDungeonEquipment" or "DeepDungeonMagicStone" or "DeepDungeonDemiclone" => 1,
            "Glasses" => 4,
            "GlassesStyle" => 15,
            "Ornament" => 8, // not part of that function, but still shifted
            _ => 0
        };

        var output = language switch
        {
            ClientLanguage.Japanese => ResolveNounJa(amount, person, attributiveSheet, row),
            ClientLanguage.English => ResolveNounEn(amount, person, attributiveSheet, row, columnOffset),
            ClientLanguage.German => ResolveNounDe(amount, person, grammaticalCase, attributiveSheet, row, columnOffset),
            ClientLanguage.French => ResolveNounFr(amount, person, attributiveSheet, row, columnOffset),
            _ => new ReadOnlySeString()
        };

        if (output.IsEmpty)
            return _empty;

        _cache.Add(key, output);
        return output;
    }

    // Component::Text::Localize::NounJa.Resolve
    private static ReadOnlySeString ResolveNounJa(int amount, int person, ExcelSheet<RawRow> attributiveSheet, RawRow row)
    {
        var builder = SeStringBuilder.SharedPool.Get();

        // Ko-So-A-Do
        var ksad = attributiveSheet.GetRow((uint)person).ReadStringColumn(amount > 1 ? 1 : 0);
        if (!ksad.IsEmpty)
            builder.Append(ksad);

        if (amount > 1)
            builder.ReplaceText("[n]"u8, Encoding.UTF8.GetBytes(amount.ToString()));

        // UnkInt5 can only be 0, because the offsets array has only 1 entry, which is 0
        var text = row.ReadStringColumn(0);
        if (!text.IsEmpty)
            builder.Append(text);

        var ross = builder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(builder);
        return ross;
    }

    // Component::Text::Localize::NounEn.Resolve
    private static ReadOnlySeString ResolveNounEn(int amount, int person, ExcelSheet<RawRow> attributiveSheet, RawRow row, int columnOffset)
    {
        /*
          a1->Offsets[0] = SingularColumnIdx
          a1->Offsets[1] = PluralColumnIdx
          a1->Offsets[2] = StartsWithVowelColumnIdx
          a1->Offsets[3] = PossessivePronounColumnIdx
          a1->Offsets[4] = ArticleColumnIdx
        */

        // UnkInt5 isn't really used here. there are only 5 offsets in the array
        // offsets = &a1->Offsets[5 * a2->UnkInt5];

        var builder = SeStringBuilder.SharedPool.Get();

        var articleIndex = row.ReadInt8Column(columnOffset + ArticleColumnIdx);
        if (articleIndex == 0)
        {
            var v14 = row.ReadInt8Column(columnOffset + StartsWithVowelColumnIdx);
            var v17 = v14 + 2 * (v14 + 1);
            var article = attributiveSheet.GetRow((uint)person).ReadStringColumn(v17 + (amount == 1 ? 0 : 1));
            if (!article.IsEmpty)
                builder.Append(article);

            // skipping link marker ("//")
        }

        var text = row.ReadStringColumn(columnOffset + (amount == 1 ? SingularColumnIdx : PluralColumnIdx));
        if (!text.IsEmpty)
            builder.Append(text);

        builder.ReplaceText("[n]"u8, Encoding.UTF8.GetBytes(amount.ToString()));

        var ross = builder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(builder);
        return ross;
    }

    // Component::Text::Localize::NounDe.Resolve
    private static ReadOnlySeString ResolveNounDe(int amount, int person, int grammaticalCase, ExcelSheet<RawRow> attributiveSheet, RawRow row, int columnOffset)
    {
        /*
             a1->Offsets[0] = SingularColumnIdx
             a1->Offsets[1] = PluralColumnIdx
             a1->Offsets[2] = PronounColumnIdx
             a1->Offsets[3] = AdjectiveColumnIdx
             a1->Offsets[4] = PossessivePronounColumnIdx
             a1->Offsets[5] = Unknown5ColumnIdx
             a1->Offsets[6] = ArticleColumnIdx
         */

        var builder = SeStringBuilder.SharedPool.Get();
        ReadOnlySeString ross;

        var readColumnDirectly = ((byte)(grammaticalCase >> 8 & 0xFF) & 1) == 1; // BYTE2(Case) & 1

        if ((grammaticalCase & 0x10000) != 0)
            grammaticalCase = 0;

        // TODO: I didn't try this out yet, see if it works
        if (readColumnDirectly)
        {
            var v15 = row.ReadStringColumn(grammaticalCase - 0x10000);
            if (!v15.IsEmpty)
                builder.Append(v15);

            builder.ReplaceText("[n]"u8, Encoding.UTF8.GetBytes(amount.ToString()));

            ross = builder.ToReadOnlySeString();
            SeStringBuilder.SharedPool.Return(builder);
            return ross;
        }

        var genderIdx = row.ReadInt8Column(columnOffset + PronounColumnIdx);
        var articleIndex = row.ReadInt8Column(columnOffset + ArticleColumnIdx);

        var caseColumnOffset = 4 * grammaticalCase + 8;
        sbyte v27;
        if (amount == 1)
        {
            var v26 = columnOffset + AdjectiveColumnIdx;
            v27 = (sbyte)(v26 >= 0 ? row.ReadInt8Column(v26) : ~v26);
        }
        else
        {
            var v29 = columnOffset + PossessivePronounColumnIdx;
            v27 = (sbyte)(v29 >= 0 ? row.ReadInt8Column(v29) : ~v29);
            genderIdx = 3;
        }

        var has_t = false; // v44, has article placeholder
        var text = row.ReadStringColumn(columnOffset + (amount == 1 ? SingularColumnIdx : PluralColumnIdx));
        if (!text.IsEmpty)
        {
            has_t = text.Contains("[t]"u8); // v34

            if (articleIndex == 0 && !has_t)
            {
                var v36 = attributiveSheet.GetRow((uint)person).ReadStringColumn(caseColumnOffset + genderIdx);
                if (!v36.IsEmpty)
                    builder.Append(v36);
            }

            // skipping link marker ("//")

            builder.Append(text);

            var v43 = attributiveSheet.GetRow((uint)(v27 + 26)).ReadStringColumn(caseColumnOffset + genderIdx);
            if (!v43.IsEmpty)
            {
                var has_p = builder.Contains("[p]"u8); // inverted v38
                if (has_p)
                    builder.ReplaceText("[p]"u8, v43);
                else
                    builder.Append(v43);
            }

            if (has_t)
            {
                var v46 = attributiveSheet.GetRow(39).ReadStringColumn(caseColumnOffset + genderIdx); // Definiter Artikel
                if (!v46.IsEmpty)
                    builder.ReplaceText("[t]"u8, v46);
            }
        }

        var v50 = attributiveSheet.GetRow(24).ReadStringColumn(caseColumnOffset + genderIdx);
        if (!v50.IsEmpty)
            builder.ReplaceText("[pa]"u8, v50);

        var v52 = attributiveSheet.GetRow(26); // Starke Flexion eines Artikels?!
        if (person is 2 or 6 || has_t) // ((Person - 2) & -5) == 0
            v52 = attributiveSheet.GetRow(25); // Schwache Flexion eines Adjektivs?!
        else if (person == 5)
            v52 = attributiveSheet.GetRow(38); // Starke Deklination
        else if (person == 1)
            v52 = attributiveSheet.GetRow(37); // Gemischte Deklination

        var v54 = v52.ReadStringColumn(caseColumnOffset + genderIdx);
        if (!v54.IsEmpty)
            builder.ReplaceText("[a]"u8, v54);

        builder.ReplaceText("[n]"u8, Encoding.UTF8.GetBytes(amount.ToString()));

        ross = builder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(builder);
        return ross;
    }

    // Component::Text::Localize::NounFr.Resolve
    private static ReadOnlySeString ResolveNounFr(int amount, int person, ExcelSheet<RawRow> attributiveSheet, RawRow row, int columnOffset)
    {
        /*
            a1->Offsets[0] = SingularColumnIdx
            a1->Offsets[1] = PluralColumnIdx
            a1->Offsets[2] = StartsWithVowelColumnIdx
            a1->Offsets[3] = PronounColumnIdx
            a1->Offsets[4] = Unknown5ColumnIdx
            a1->Offsets[5] = ArticleColumnIdx
        */

        // UnkInt5 isn't really used here either. there are only 6 offsets in the array
        // UnkInt5--;
        // offsets = &a1->Offsets[6 * a2->UnkInt5];

        var builder = SeStringBuilder.SharedPool.Get();
        ReadOnlySeString ross;

        var v33 = row.ReadInt8Column(columnOffset + StartsWithVowelColumnIdx);
        var v15 = row.ReadInt8Column(columnOffset + PronounColumnIdx);
        var v19 = row.ReadInt8Column(columnOffset + ArticleColumnIdx);
        var v20 = 4 * (v33 + 6 + 2 * v15);

        if (v19 != 0)
        {
            var v21 = attributiveSheet.GetRow((uint)person).ReadStringColumn(v20);
            if (!v21.IsEmpty)
                builder.Append(v21);

            // skipping link marker ("//")

            var v30 = row.ReadStringColumn(columnOffset + (amount <= 1 ? SingularColumnIdx : PluralColumnIdx));
            if (!v30.IsEmpty)
                builder.Append(v30);

            if (amount <= 1)
                builder.ReplaceText("[n]"u8, Encoding.UTF8.GetBytes(amount.ToString()));

            ross = builder.ToReadOnlySeString();
            SeStringBuilder.SharedPool.Return(builder);
            return ross;
        }

        var v17 = row.ReadInt8Column(columnOffset + Unknown5ColumnIdx);
        if (v17 != 0 && (amount > 1 || v17 == 2))
        {
            var v29 = attributiveSheet.GetRow((uint)person).ReadStringColumn(v20 + 2);
            if (!v29.IsEmpty)
            {
                builder.Append(v29);

                // skipping link marker ("//")

                var v30 = row.ReadStringColumn(columnOffset + PluralColumnIdx);
                if (!v30.IsEmpty)
                    builder.Append(v30);
            }
        }
        else
        {
            var v27 = attributiveSheet.GetRow((uint)person).ReadStringColumn(v20 + (v17 != 0 ? 1 : 3));
            if (!v27.IsEmpty)
                builder.Append(v27);

            // skipping link marker ("//")

            var v30 = row.ReadStringColumn(columnOffset + SingularColumnIdx);
            if (!v30.IsEmpty)
                builder.Append(v30);
        }

        builder.ReplaceText("[n]"u8, Encoding.UTF8.GetBytes(amount.ToString()));

        ross = builder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(builder);
        return ross;
    }
}
