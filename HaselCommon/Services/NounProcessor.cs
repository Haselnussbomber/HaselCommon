using System.Collections.Concurrent;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

/*
Attributive sheet:
  Japanese:
    Unknown0 = Singular Demonstrative
    Unknown1 = Plural Demonstrative
  English:
    Unknown2 = Article before a singular noun beginning with a consonant sound
    Unknown3 = Article before a generic noun beginning with a consonant sound
    Unknown4 = N/A
    Unknown5 = Article before a singular noun beginning with a vowel sound
    Unknown6 = Article before a generic noun beginning with a vowel sound
    Unknown7 = N/A
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
  French (unsure):
    Unknown24 = Singular Article
    Unknown25 = Singular Masculine Article
    Unknown26 = Plural Masculine Article
    Unknown27 = ?
    Unknown28 = ?
    Unknown29 = Singular Masculine/Feminine Article, before a noun beginning in a vowel or an h
    Unknown30 = Plural Masculine/Feminine Article, before a noun beginning in a vowel or an h
    Unknown31 = ?
    Unknown32 = ?
    Unknown33 = Singular Feminine Article
    Unknown34 = Plural Feminine Article
    Unknown35 = ?
    Unknown36 = ?
    Unknown37 = Singular Masculine/Feminine Article, before a noun beginning in a vowel or an h
    Unknown38 = Plural Masculine/Feminine Article, before a noun beginning in a vowel or an h
    Unknown39 = ?
    Unknown40 = ?
*/

public enum JapaneseArticleType
{
    /// <summary>
    /// Near listener (それら).
    /// </summary>
    NearListener = 1,

    /// <summary>
    /// Distant from both speaker and listener (あれら).
    /// </summary>
    Distant = 2,
}

public enum EnglishArticleType
{
    /// <summary>
    /// Indefinite article (a, an).
    /// </summary>
    Indefinite = 1,

    /// <summary>
    /// Definite article (the).
    /// </summary>
    Definite = 2,
}

public enum GermanArticleType
{
    /// <summary>
    /// Unbestimmter Artikel (ein, eine, etc.).
    /// </summary>
    Indefinite = 1,

    /// <summary>
    /// Bestimmter Artikel (der, die, das, etc.).
    /// </summary>
    Definite = 2,

    /// <summary>
    /// Possessivartikel "dein" (dein, deine, etc.).
    /// </summary>
    Possessive = 3,

    /// <summary>
    /// Negativartikel "kein" (kein, keine, etc.).
    /// </summary>
    Negative = 4,

    /// <summary>
    /// Nullartikel.
    /// </summary>
    ZeroArticle = 5,

    /// <summary>
    /// Demonstrativpronomen "dieser" (dieser, diese, etc.).
    /// </summary>
    Demonstrative = 6,
}

public enum FrenchArticleType
{
    /// <summary>
    /// Indefinite article (une, des).
    /// </summary>
    Indefinite = 1,

    /// <summary>
    /// Definite article (le, la, les).
    /// </summary>
    Definite = 2,

    /// <summary>
    /// Possessive article (mon, mes).
    /// </summary>
    PossessiveFirstPerson = 3,

    /// <summary>
    /// Possessive article (ton, tes).
    /// </summary>
    PossessiveSecondPerson = 4,

    /// <summary>
    /// Possessive article (son, ses).
    /// </summary>
    PossessiveThirdPerson = 5,
}

/// <summary>
/// Provides functionality to process texts from sheets containing grammatical placeholders.
/// </summary>
/// <param name="logger">An instance of <see cref="ILogger"/> used for logging warnings.</param>
/// <param name="excelModule">An instance of <see cref="ExcelModule"/> providing access to sheet data.</param>
[RegisterSingleton]
public class NounProcessor(ILogger<NounProcessor> logger, IDataManager dataManager)
{
    private readonly ConcurrentDictionary<(
        Language Language,
        string SheetName,
        uint RowId,
        int Quantity,
        int ArticleType,
        int GrammaticalCase
    ), ReadOnlySeString> _cache = [];

    // column names from ExdSchema, most likely incorrect
    private const int SingularColumnIdx = 0;
    private const int AdjectiveColumnIdx = 1;
    private const int PluralColumnIdx = 2;
    private const int PossessivePronounColumnIdx = 3;
    private const int StartsWithVowelColumnIdx = 4;
    private const int Unknown5ColumnIdx = 5; // probably used in Chinese texts
    private const int PronounColumnIdx = 6;
    private const int ArticleColumnIdx = 7;

    // Placeholders:
    // [t] = article or grammatical gender (EN: the, DE: der, die, das)
    // [n] = amount (number)
    // [a] = declension
    // [p] = plural
    // [pa] = ?

    /// <summary>
    /// Processes a specific row from a sheet and generates a formatted string based on grammatical and language-specific rules.
    /// </summary>
    /// <param name="language">The language of the sheet to be processed.</param>
    /// <param name="sheetName">The name of the sheet containing the row to process.</param>
    /// <param name="rowId">The row id within the sheet to process.</param>
    /// <param name="quantity">The quantity of the entity (default is 1). Used to determine grammatical number (e.g., singular or plural).</param>
    /// <param name="person">The grammatical person. Mainly used for German and French texts. 5 is neutral.</param>
    /// <param name="grammaticalCase">The grammatical case (e.g., Nominative, Genitive, Dative, Accusative) used for German texts (default is 0).</param>
    /// <param name="linkMarker">An optional string that is placed in front of the text that should be linked, such as item names (default is an empty string; the game uses "//").</param>
    /// <returns>A ReadOnlySeString representing the processed text.</returns>
    public ReadOnlySeString ProcessRow(
        string sheetName,
        uint rowId,
        Language? language = null,
        int quantity = 1,
        int articleType = 1,
        int grammaticalCase = 0,
        ReadOnlySeString linkMarker = default)
    {
        var lang = language ?? dataManager.GameData.Options.DefaultExcelLanguage;

        if (_cache.TryGetValue((lang, sheetName, rowId, quantity, articleType, grammaticalCase), out var value))
            return value;

        var sheet = dataManager.Excel.GetSheet<RawRow>(lang, sheetName);
        if (sheet == null)
        {
            logger.LogWarning("Sheet {SheetName} not found", sheetName);
            return default;
        }

        if (!sheet.HasRow(rowId))
        {
            logger.LogWarning("Sheet {SheetName} does not contain row #{RowId}", sheetName, rowId);
            return default;
        }

        var row = sheet.GetRow(rowId);

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

        return ProcessRow(sheetName, row, lang, columnOffset, quantity, articleType, grammaticalCase, linkMarker);
    }

    /// <summary>
    /// Processes a specific row and generates a formatted string based on grammatical and language-specific rules.
    /// </summary>
    /// <param name="sheetName">The name of the sheet containing the row to process. Used for caching.</param>
    /// <param name="row">The row to process.</param>
    /// <param name="language">The language of the row to be processed. If null, <see cref="LuminaOptions.DefaultExcelLanguage"/> is used.</param>
    /// <param name="columnOffset">The starting position offset for the text-related columns within the row.</param>
    /// <param name="quantity">The quantity of the entity, used to determine grammatical number (e.g., singular or plural; default is 1).</param>
    /// <param name="person">The grammatical person. Mainly used for German and French texts. 5 is neutral.</param>
    /// <param name="grammaticalCase">The grammatical case (e.g., Nominative, Genitive, Dative, Accusative) used for German texts (default is 0).</param>
    /// <param name="linkMarker">An optional string that is placed in front of the text that should be linked, such as item names (default is an empty string; the game uses "//").</param>
    /// <returns>A ReadOnlySeString representing the processed text.</returns>
    public ReadOnlySeString ProcessRow(
        string sheetName,
        RawRow row,
        Language? language = null,
        int columnOffset = 0,
        int quantity = 1,
        int articleType = 1,
        int grammaticalCase = 0,
        ReadOnlySeString linkMarker = default)
    {
        var lang = language ?? dataManager.GameData.Options.DefaultExcelLanguage;

        if ((short)grammaticalCase < 0 || (short)grammaticalCase > 5)
            return default;

        var key = (lang, sheetName, row.RowId, quantity, articleType, grammaticalCase);

        if (_cache.TryGetValue(key, out var value))
            return value;

        var attributiveSheet = dataManager.Excel.GetSheet<RawRow>(lang, "Attributive");
        if (attributiveSheet == null)
        {
            logger.LogWarning("Sheet Attributive not found");
            return default;
        }

        var output = lang switch
        {
            Language.Japanese => ResolveNounJa(row, quantity, (JapaneseArticleType)articleType, attributiveSheet, linkMarker, columnOffset),
            Language.English => ResolveNounEn(row, quantity, (EnglishArticleType)articleType, attributiveSheet, linkMarker, columnOffset),
            Language.German => ResolveNounDe(row, quantity, (GermanArticleType)articleType, attributiveSheet, linkMarker, columnOffset, (ushort)grammaticalCase),
            Language.French => ResolveNounFr(row, quantity, (FrenchArticleType)articleType, attributiveSheet, linkMarker, columnOffset),
            _ => default
        };

        _cache.TryAdd(key, output);

        return output;
    }

    /// <summary>
    /// Resolves noun placeholders in Japanese text.
    /// </summary>
    /// <param name="row">The row containing the unprocessed text.</param>
    /// <param name="quantity">The grammatical number representing the quantity.</param>
    /// <param name="person">The grammatical person (e.g., 1 for first person, 2 for second person).</param>
    /// <param name="attributiveSheet">The sheet containing the data used for resolving the noun.</param>
    /// <param name="linkMarker">An optional prefix string for linked text, such as item names.</param>
    /// <param name="columnOffset">The starting position offset for the text-related columns within the row.</param>
    /// <returns>A ReadOnlySeString representing the processed text.</returns>
    /// <remarks>
    /// This is a C# implementation of Component::Text::Localize::NounJa.Resolve.
    /// </remarks>
    private static ReadOnlySeString ResolveNounJa(RawRow row, int quantity, JapaneseArticleType articleType, ExcelSheet<RawRow> attributiveSheet, ReadOnlySeString linkMarker, int columnOffset)
    {
        var builder = SeStringBuilder.SharedPool.Get();

        // Ko-So-A-Do
        var ksad = attributiveSheet.GetRow((uint)articleType).ReadStringColumn(quantity > 1 ? 1 : 0);
        if (!ksad.IsEmpty)
        {
            builder.Append(ksad);

            if (quantity > 1)
            {
                ReplaceText(ref builder, "[n]"u8, ReadOnlySeString.FromText(quantity.ToString()));
            }
        }

        if (!linkMarker.IsEmpty)
            builder.Append(linkMarker);

        var text = row.ReadStringColumn(columnOffset);
        if (!text.IsEmpty)
            builder.Append(text);

        var ross = builder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(builder);
        return ross;
    }

    /// <summary>
    /// Resolves noun placeholders in English text.
    /// </summary>
    /// <param name="row">The row containing the unprocessed text.</param>
    /// <param name="quantity">The grammatical number representing the quantity.</param>
    /// <param name="person">The grammatical person (e.g., 1 for first person, 2 for second person).</param>
    /// <param name="attributiveSheet">The sheet containing the data used for resolving the noun.</param>
    /// <param name="linkMarker">An optional prefix string for linked text, such as item names.</param>
    /// <param name="columnOffset">The starting position offset for the text-related columns within the row.</param>
    /// <returns>A ReadOnlySeString representing the processed text.</returns>
    /// <remarks>
    /// This is a C# implementation of Component::Text::Localize::NounEn.Resolve.
    /// </remarks>
    public static ReadOnlySeString ResolveNounEn(RawRow row, int quantity, EnglishArticleType articleType, ExcelSheet<RawRow> attributiveSheet, ReadOnlySeString linkMarker, int columnOffset)
    {
        /*
          a1->Offsets[0] = SingularColumnIdx
          a1->Offsets[1] = PluralColumnIdx
          a1->Offsets[2] = StartsWithVowelColumnIdx
          a1->Offsets[3] = PossessivePronounColumnIdx
          a1->Offsets[4] = ArticleColumnIdx
        */

        var builder = SeStringBuilder.SharedPool.Get();

        var isProperNounColumn = columnOffset + ArticleColumnIdx;
        var isProperNoun = isProperNounColumn >= 0 ? row.ReadInt8Column(isProperNounColumn) : ~isProperNounColumn;
        if (isProperNoun == 0)
        {
            var startsWithVowelColumn = columnOffset + StartsWithVowelColumnIdx;
            var startsWithVowel = startsWithVowelColumn >= 0 ? row.ReadInt8Column(startsWithVowelColumn) : ~startsWithVowelColumn;

            var articleColumn = startsWithVowel + 2 * (startsWithVowel + 1);
            var grammaticalNumberColumnOffset = quantity == 1 ? SingularColumnIdx : PluralColumnIdx;
            var article = attributiveSheet.GetRow((uint)articleType).ReadStringColumn(articleColumn + grammaticalNumberColumnOffset);
            if (!article.IsEmpty)
                builder.Append(article);

            if (!linkMarker.IsEmpty)
                builder.Append(linkMarker);
        }

        var text = row.ReadStringColumn(columnOffset + (quantity == 1 ? SingularColumnIdx : PluralColumnIdx));
        if (!text.IsEmpty)
            builder.Append(text);

        ReplaceText(ref builder, "[n]"u8, ReadOnlySeString.FromText(quantity.ToString()));

        var ross = builder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(builder);
        return ross;
    }

    /// <summary>
    /// Resolves noun placeholders in German text.
    /// </summary>
    /// <param name="row">The row containing the unprocessed text.</param>
    /// <param name="quantity">The grammatical number representing the quantity.</param>
    /// <param name="person">The grammatical person (e.g., 1 for first person, 2 for second person).</param>
    /// <param name="attributiveSheet">The sheet containing the data used for resolving the noun.</param>
    /// <param name="linkMarker">An optional prefix string for linked text, such as item names.</param>
    /// <param name="columnOffset">The starting position offset for the text-related columns within the row.</param>
    /// <param name="grammaticalCase">The grammatical case (e.g., Nominative, Genitive, Dative, Accusative; default is 0).</param>
    /// <returns>A ReadOnlySeString representing the processed text.</returns>
    /// <remarks>
    /// This is a C# implementation of Component::Text::Localize::NounDe.Resolve.
    /// </remarks>
    public static ReadOnlySeString ResolveNounDe(RawRow row, int quantity, GermanArticleType articleType, ExcelSheet<RawRow> attributiveSheet, ReadOnlySeString linkMarker, int columnOffset, ushort grammaticalCase)
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

        var readColumnDirectly = (byte)(grammaticalCase >> 8 & 0xFF) & 1; // BYTE2(Case) & 1

        if ((grammaticalCase & 0x10000) != 0)
            grammaticalCase = 0;

        if (readColumnDirectly != 0)
        {
            builder.Append(row.ReadStringColumn(grammaticalCase - 0x10000));
            ReplaceText(ref builder, "[n]"u8, ReadOnlySeString.FromText(quantity.ToString()));

            ross = builder.ToReadOnlySeString();
            SeStringBuilder.SharedPool.Return(builder);
            return ross;
        }

        var genderIndexColumn = columnOffset + PronounColumnIdx;
        var genderIndex = genderIndexColumn >= 0 ? row.ReadInt8Column(genderIndexColumn) : ~genderIndexColumn;

        var articleIndexColumn = columnOffset + ArticleColumnIdx;
        var articleIndex = articleIndexColumn >= 0 ? row.ReadInt8Column(articleIndexColumn) : ~articleIndexColumn;

        var caseColumnOffset = 4 * grammaticalCase + 8;

        var caseRowOffsetColumn = columnOffset + (quantity == 1 ? AdjectiveColumnIdx : PossessivePronounColumnIdx);
        var caseRowOffset = caseRowOffsetColumn >= 0 ? row.ReadInt8Column(caseRowOffsetColumn) : (sbyte)~caseRowOffsetColumn;

        if (quantity != 1)
            genderIndex = 3;

        var has_t = false;
        var text = row.ReadStringColumn(columnOffset + (quantity == 1 ? SingularColumnIdx : PluralColumnIdx));
        if (!text.IsEmpty)
        {
            has_t = ContainsText(text, "[t]"u8);

            if (articleIndex == 0 && !has_t)
            {
                var grammaticalGender = attributiveSheet.GetRow((uint)articleType).ReadStringColumn(caseColumnOffset + genderIndex); // Genus
                if (!grammaticalGender.IsEmpty)
                    builder.Append(grammaticalGender);
            }

            if (!linkMarker.IsEmpty)
                builder.Append(linkMarker);

            builder.Append(text);

            var plural = attributiveSheet.GetRow((uint)(caseRowOffset + 26)).ReadStringColumn(caseColumnOffset + genderIndex);
            if (ContainsText(builder, "[p]"u8))
                ReplaceText(ref builder, "[p]"u8, plural);
            else
                builder.Append(plural);

            if (has_t)
            {
                var article = attributiveSheet.GetRow(39).ReadStringColumn(caseColumnOffset + genderIndex); // Definiter Artikel
                ReplaceText(ref builder, "[t]"u8, article);
            }
        }

        var pa = attributiveSheet.GetRow(24).ReadStringColumn(caseColumnOffset + genderIndex);
        ReplaceText(ref builder, "[pa]"u8, pa);

        RawRow declensionRow;

        if (articleType is GermanArticleType.Possessive or GermanArticleType.Demonstrative || has_t)
            declensionRow = attributiveSheet.GetRow(25); // Schwache Flexion eines Adjektivs?!
        else if (articleType == GermanArticleType.ZeroArticle)
            declensionRow = attributiveSheet.GetRow(38); // Starke Deklination
        else if (articleType == GermanArticleType.Definite)
            declensionRow = attributiveSheet.GetRow(37); // Gemischte Deklination
        else
            declensionRow = attributiveSheet.GetRow(26); // Starke Flexion eines Artikels?!

        var declension = declensionRow.ReadStringColumn(caseColumnOffset + genderIndex);
        ReplaceText(ref builder, "[a]"u8, declension);

        ReplaceText(ref builder, "[n]"u8, ReadOnlySeString.FromText(quantity.ToString()));

        ross = builder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(builder);
        return ross;
    }

    // Component::Text::Localize::.Resolve
    /// <summary>
    /// Resolves noun placeholders in French text.
    /// </summary>
    /// <param name="row">The row containing the unprocessed text.</param>
    /// <param name="quantity">The grammatical number representing the quantity.</param>
    /// <param name="person">The grammatical person (e.g., 1 for first person, 2 for second person).</param>
    /// <param name="attributiveSheet">The sheet containing the data used for resolving the noun.</param>
    /// <param name="linkMarker">An optional prefix string for linked text, such as item names.</param>
    /// <param name="columnOffset">The starting position offset for the text-related columns within the row.</param>
    /// <returns>A ReadOnlySeString representing the processed text.</returns>
    /// <remarks>
    /// This is a C# implementation of Component::Text::Localize::NounFr.Resolve.
    /// </remarks>
    public static ReadOnlySeString ResolveNounFr(RawRow row, int quantity, FrenchArticleType articleType, ExcelSheet<RawRow> attributiveSheet, ReadOnlySeString linkMarker, int columnOffset)
    {
        /*
            a1->Offsets[0] = SingularColumnIdx
            a1->Offsets[1] = PluralColumnIdx
            a1->Offsets[2] = StartsWithVowelColumnIdx
            a1->Offsets[3] = PronounColumnIdx
            a1->Offsets[4] = Unknown5ColumnIdx
            a1->Offsets[5] = ArticleColumnIdx
        */

        var builder = SeStringBuilder.SharedPool.Get();
        ReadOnlySeString ross;

        var startsWithVowelColumn = columnOffset + StartsWithVowelColumnIdx;
        var startsWithVowel = startsWithVowelColumn >= 0 ? row.ReadInt8Column(startsWithVowelColumn) : ~startsWithVowelColumn;

        var pronounColumn = columnOffset + PronounColumnIdx;
        var pronoun = pronounColumn >= 0 ? row.ReadInt8Column(pronounColumn) : ~pronounColumn;

        var articleColumn = columnOffset + ArticleColumnIdx;
        var article = articleColumn >= 0 ? row.ReadInt8Column(articleColumn) : ~articleColumn;

        var v20 = 4 * (startsWithVowel + 6 + 2 * pronoun);

        if (article != 0)
        {
            var v21 = attributiveSheet.GetRow((uint)articleType).ReadStringColumn(v20);
            if (!v21.IsEmpty)
                builder.Append(v21);

            if (!linkMarker.IsEmpty)
                builder.Append(linkMarker);

            var text = row.ReadStringColumn(columnOffset + (quantity <= 1 ? SingularColumnIdx : PluralColumnIdx));
            if (!text.IsEmpty)
                builder.Append(text);

            if (quantity <= 1)
                ReplaceText(ref builder, "[n]"u8, ReadOnlySeString.FromText(quantity.ToString()));

            ross = builder.ToReadOnlySeString();
            SeStringBuilder.SharedPool.Return(builder);
            return ross;
        }

        var v17 = row.ReadInt8Column(columnOffset + Unknown5ColumnIdx);
        if (v17 != 0 && (quantity > 1 || v17 == 2))
        {
            var v29 = attributiveSheet.GetRow((uint)articleType).ReadStringColumn(v20 + 2);
            if (!v29.IsEmpty)
            {
                builder.Append(v29);

                if (!linkMarker.IsEmpty)
                    builder.Append(linkMarker);

                var text = row.ReadStringColumn(columnOffset + PluralColumnIdx);
                if (!text.IsEmpty)
                    builder.Append(text);
            }
        }
        else
        {
            var v27 = attributiveSheet.GetRow((uint)articleType).ReadStringColumn(v20 + (v17 != 0 ? 1 : 3));
            if (!v27.IsEmpty)
                builder.Append(v27);

            if (!linkMarker.IsEmpty)
                builder.Append(linkMarker);

            var text = row.ReadStringColumn(columnOffset + SingularColumnIdx);
            if (!text.IsEmpty)
                builder.Append(text);
        }

        ReplaceText(ref builder, "[n]"u8, ReadOnlySeString.FromText(quantity.ToString()));

        ross = builder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(builder);
        return ross;
    }

    private static bool ContainsText(ReadOnlySeString ross, ReadOnlySpan<byte> needle)
    {
        foreach (var payload in ross)
        {
            if (payload.Type != ReadOnlySePayloadType.Text)
                continue;

            if (payload.Body.Span.IndexOf(needle) != -1)
                return true;
        }

        return false;
    }

    private static bool ContainsText(SeStringBuilder builder, ReadOnlySpan<byte> needle)
    {
        return ContainsText(builder.ToReadOnlySeString(), needle);
    }

    private static ReadOnlySeString ReplaceText(ReadOnlySeString ross, ReadOnlySpan<byte> toFind, ReadOnlySpan<byte> replacement)
    {
        if (ross.IsEmpty)
            return ross;

        var sb = SeStringBuilder.SharedPool.Get();

        foreach (var payload in ross)
        {
            if (payload.Type == ReadOnlySePayloadType.Invalid)
                continue;

            if (payload.Type != ReadOnlySePayloadType.Text)
            {
                sb.Append(payload);
                continue;
            }

            var index = payload.Body.Span.IndexOf(toFind);
            if (index == -1)
            {
                sb.Append(payload);
                continue;
            }

            var lastIndex = 0;
            while (index != -1)
            {
                sb.Append(payload.Body.Span[lastIndex..index]);

                if (!replacement.IsEmpty)
                {
                    sb.Append(replacement);
                }

                lastIndex = index + toFind.Length;
                index = payload.Body.Span[lastIndex..].IndexOf(toFind);

                if (index != -1)
                    index += lastIndex;
            }

            sb.Append(payload.Body.Span[lastIndex..]);
        }

        var output = sb.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(sb);
        return output;
    }

    private static void ReplaceText(ref SeStringBuilder builder, ReadOnlySpan<byte> toFind, ReadOnlySpan<byte> replacement)
    {
        if (toFind.IsEmpty)
            return;

        var str = builder.ToReadOnlySeString();
        if (str.IsEmpty)
            return;

        builder.Clear();
        builder.Append(ReplaceText(str, toFind, replacement));
    }
}
