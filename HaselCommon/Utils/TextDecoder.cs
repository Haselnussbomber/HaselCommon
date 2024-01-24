using System.Collections.Generic;
using Dalamud;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.System.String;
using HaselCommon.Extensions;
using HaselCommon.Structs.Internal;
using Lumina.Excel;
using LuminaSeString = Lumina.Text.SeString;

namespace HaselCommon.Utils;

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

public static unsafe class TextDecoder
{
    private static readonly Dictionary<(ClientLanguage Language, string SheetName, uint RowId, int Amount, uint Person, uint Case), string> _cache = [];

    private const int SingularColumnIdx = 0;
    private const int AdjectiveColumnIdx = 1;
    private const int PluralColumnIdx = 2;
    private const int PossessivePronounColumnIdx = 3;
    private const int StartsWithVowelColumnIdx = 4;
    private const int Unknown5ColumnIdx = 5;
    private const int PronounColumnIdx = 6;
    private const int ArticleColumnIdx = 7;

    // <XXnoun(SheetName,Person,RowId,Amount,Case[,UnkInt5])>
    public static string ProcessNoun(string SheetName, uint RowId, int Amount = 1, uint Person = 5, uint Case = 1)
    {
        var language = Service.TranslationManager.ClientLanguage;

        // UnkInt5--;
        Case--;

        if (Case is > 5 || (language != ClientLanguage.German && Case > 0))
            return string.Empty;

        var key = (language, SheetName, RowId, Amount, Person, Case);
        if (_cache.TryGetValue(key, out var value))
            return value;

        var attributiveSheet = Service.DataManager.GameData.Excel.GetSheetRaw("Attributive", language.ToLumina());
        if (attributiveSheet == null)
        {
            Service.PluginLog.Warning("Sheet Attributive not found");
            return string.Empty;
        }

        var sheet = Service.DataManager.GameData.Excel.GetSheetRaw(SheetName, language.ToLumina());
        if (sheet == null)
        {
            Service.PluginLog.Warning("Sheet {SheetName} not found", SheetName);
            return string.Empty;
        }

        var row = sheet.GetRow(RowId);
        if (row == null)
        {
            Service.PluginLog.Warning("Sheet {SheetName} does not contain row #{RowId}", SheetName, RowId);
            return string.Empty;
        }

        var columnOffset = SheetName switch
        {
            "BeastTribe" => 10,
            "DeepDungeonItem" or "DeepDungeonEquipment" or "DeepDungeonMagicStone" or "DeepDungeonDemiclone" => 1,
            _ => 0
        };

        var output = language switch
        {
            ClientLanguage.Japanese => ResolveNounJa(Amount, Person, attributiveSheet, row),
            ClientLanguage.English => ResolveNounEn(Amount, Person, attributiveSheet, row, columnOffset),
            ClientLanguage.German => ResolveNounDe(Amount, Person, Case, attributiveSheet, row, columnOffset),
            ClientLanguage.French => ResolveNounFr(Amount, Person, attributiveSheet, row, columnOffset),
            _ => null
        };

        if (output == null)
            return string.Empty;

        var str = SeString.Parse(output->StringPtr, (int)output->BufUsed - 1).ToString();
        _cache.Add(key, str);
        output->Dtor(true);
        return str;
    }

    // Component::Text::Localize::NounJa.Resolve
    private static Utf8String* ResolveNounJa(int Amount, uint Person, RawExcelSheet attributiveSheet, RowParser row)
    {
        var output = Utf8String.CreateEmpty();
        var placeholder = Utf8String.CreateEmpty();
        var temp = Utf8String.CreateEmpty();

        var v13 = attributiveSheet.GetRow(Person)?.ReadColumn<LuminaSeString>(Amount > 1 ? 1 : 0);
        if (v13 != null)
            output->SetString(v13.RawData.WithNullTerminator());

        if (Amount > 1)
        {
            var v16 = Amount.ToString();
            placeholder->SetString("[n]");
            temp->SetString(v16);
            Statics.Utf8StringReplace(output, placeholder, temp);
        }

        // UnkInt5 can only be 0, because the offsets array has only 1 entry, which is 0
        var v21 = row.ReadColumn<LuminaSeString>(0);
        if (v21 != null)
        {
            temp->SetString(v21.RawData.WithNullTerminator());
            output->Append(temp);
        }

        placeholder->Dtor(true);
        temp->Dtor(true);

        return output;
    }

    // Component::Text::Localize::NounEn.Resolve
    private static Utf8String* ResolveNounEn(int Amount, uint Person, RawExcelSheet attributiveSheet, RowParser row, int columnOffset)
    {
        var output = Utf8String.CreateEmpty();
        var placeholder = Utf8String.CreateEmpty();
        var temp = Utf8String.CreateEmpty();

        /*
          a1->Offsets[0] = SingularColumnIdx
          a1->Offsets[1] = PluralColumnIdx
          a1->Offsets[2] = StartsWithVowelColumnIdx
          a1->Offsets[3] = PossessivePronounColumnIdx
          a1->Offsets[4] = ArticleColumnIdx
        */

        // UnkInt5 isn't really used here. there are only 5 offsets in the array
        // offsets = &a1->Offsets[5 * a2->UnkInt5];

        var v13 = columnOffset + StartsWithVowelColumnIdx;
        var v14 = v13 >= 0 ? row.ReadColumn<sbyte>(v13) : ~v13;
        var v15 = columnOffset + ArticleColumnIdx;
        var v16 = v15 >= 0 ? row.ReadColumn<sbyte>(v15) : ~v15;

        var textColumnIdx = Amount == 1 ? SingularColumnIdx : PluralColumnIdx;

        if (v16 != 0)
        {
            var v32 = row.ReadColumn<LuminaSeString>(columnOffset + textColumnIdx);
            if (v32 != null)
                output->SetString(v32.RawData.WithNullTerminator());
        }
        else
        {
            var v17 = v14 + 2 * (v14 + 1);
            var v22 = attributiveSheet.GetRow(Person)?.ReadColumn<LuminaSeString>(v17 + (Amount == 1 ? 0 : 1));
            if (v22 != null)
                output->SetString(v22.RawData.WithNullTerminator());

            // skipping link marker ("//")

            var v26 = row.ReadColumn<LuminaSeString>(columnOffset + textColumnIdx);
            if (v26 != null)
            {
                temp->SetString(v26.RawData.WithNullTerminator());
                output->Append(temp);
            }
        }

        var v27 = Amount.ToString();
        placeholder->SetString("[n]");
        temp->SetString(v27);
        Statics.Utf8StringReplace(output, placeholder, temp);

        placeholder->Dtor(true);
        temp->Dtor(true);

        return output;
    }

    // Component::Text::Localize::NounDe.Resolve
    private static Utf8String* ResolveNounDe(int Amount, uint Person, uint Case, RawExcelSheet attributiveSheet, RowParser row, int columnOffset)
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

        var v9 = ((byte)((Case >> 8) & 0xFF) & 1) == 1; // BYTE2(suffix) & 1; // flag to use sheets info?

        if ((Case & 0x10000) != 0)
            Case = 0;

        if (v9)
        {
            Service.PluginLog.Warning("Case with byte 2 set not implemented");
            return null;
        }

        var output = Utf8String.CreateEmpty();
        var placeholder = Utf8String.CreateEmpty();
        var temp = Utf8String.CreateEmpty();

        var textColumnIdx = Amount == 1 ? SingularColumnIdx : PluralColumnIdx;

        var v20 = columnOffset + PronounColumnIdx;
        var genderIdx = v20 >= 0 ? row.ReadColumn<sbyte>(v20) : ~v20; // can this even happen
        var v22 = columnOffset + ArticleColumnIdx;
        var articleIndex = v22 >= 0 ? row.ReadColumn<sbyte>(v22) : ~v22;

        var caseColumnOffset = (int)(4 * Case + 8);
        sbyte v27 = 0;
        if (Amount == 1)
        {
            var v26 = columnOffset + AdjectiveColumnIdx;
            v27 = (sbyte)(v26 >= 0 ? row.ReadColumn<sbyte>(v26) : ~v26);
        }
        else
        {
            var v29 = columnOffset + PossessivePronounColumnIdx;
            v27 = (sbyte)(v29 >= 0 ? row.ReadColumn<sbyte>(v29) : ~v29);
            genderIdx = 3;
        }

        var has_t = false; // v44, has article placeholder
        var v32 = row.ReadColumn<LuminaSeString>(columnOffset + textColumnIdx);
        if (v32 != null)
        {
            placeholder->SetString("[t]");
            temp->SetString(v32.RawData.WithNullTerminator());
            has_t = Statics.Utf8StringIndexOf(temp, placeholder) != -1; // v34
            output->Clear();

            if (articleIndex == 0 && !has_t)
            {
                var v36 = attributiveSheet.GetRow(Person)?.ReadColumn<LuminaSeString>(caseColumnOffset + genderIdx);
                if (v36 != null)
                    output->SetString(v36.RawData.WithNullTerminator());
            }

            // skipping link marker ("//") (processed in "E8 ?? ?? ?? ?? 41 F6 86 ?? ?? ?? ?? ?? 74 1D")

            temp->SetString(v32.RawData.WithNullTerminator());
            output->Append(temp);

            var v37 = caseColumnOffset + genderIdx;
            var v43 = attributiveSheet.GetRow((uint)(v27 + 26))?.ReadColumn<LuminaSeString>(v37);
            if (v43 != null)
            {
                placeholder->SetString("[p]");
                temp->SetString(v43.RawData.WithNullTerminator());
                var has_p = Statics.Utf8StringIndexOf(output, placeholder) != -1; // inverted v38
                if (has_p)
                    Statics.Utf8StringReplace(output, placeholder, temp);
                else
                    output->Append(temp);
            }

            if (has_t)
            {
                var v46 = attributiveSheet.GetRow(39)?.ReadColumn<LuminaSeString>(v37); // Definiter Artikel
                if (v46 != null)
                {
                    placeholder->SetString("[t]");
                    temp->SetString(v46.RawData.WithNullTerminator());
                    Statics.Utf8StringReplace(output, placeholder, temp);
                }
            }
        }

        var v48 = caseColumnOffset + genderIdx;
        var v50 = attributiveSheet.GetRow(24)?.ReadColumn<LuminaSeString>(v48);
        if (v50 != null)
        {
            placeholder->SetString("[pa]");
            temp->SetString(v50.RawData.WithNullTerminator());
            Statics.Utf8StringReplace(output, placeholder, temp);
        }

        var v52 = attributiveSheet.GetRow(26); // Starke Flexion eines Artikels?!
        if ((Person is 2 or 6) || has_t) // ((Person - 2) & -5) == 0
            v52 = attributiveSheet.GetRow(25); // Schwache Flexion eines Adjektivs?!
        else if (Person == 5)
            v52 = attributiveSheet.GetRow(38); // Starke Deklination
        else if (Person == 1)
            v52 = attributiveSheet.GetRow(37); // Gemischte Deklination

        var v54 = v52?.ReadColumn<LuminaSeString>(v48);
        if (v54 != null)
        {
            placeholder->SetString("[a]");
            temp->SetString(v54.RawData.WithNullTerminator());
            Statics.Utf8StringReplace(output, placeholder, temp);
        }

        var v55 = Amount.ToString(); // TODO: check what "E8 ?? ?? ?? ?? 44 3B F7 0F 8E" does exactly
        placeholder->SetString("[n]");
        temp->SetString(v55);
        Statics.Utf8StringReplace(output, placeholder, temp);

        placeholder->Dtor(true);
        temp->Dtor(true);

        return output;
    }

    // Component::Text::Localize::NounFr.Resolve
    private static Utf8String* ResolveNounFr(int Amount, uint Person, RawExcelSheet attributiveSheet, RowParser row, int columnOffset)
    {
        var output = Utf8String.CreateEmpty();
        var placeholder = Utf8String.CreateEmpty();
        var temp = Utf8String.CreateEmpty();

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

        var textColumnIdx = Amount <= 1 ? SingularColumnIdx : PluralColumnIdx;

        var v13 = columnOffset + StartsWithVowelColumnIdx;
        var v33 = v13 >= 0 ? row.ReadColumn<sbyte>(v13) : ~v13;
        var v14 = columnOffset + PronounColumnIdx;
        var v15 = v14 >= 0 ? row.ReadColumn<sbyte>(v14) : ~v14;
        var v16 = columnOffset + Unknown5ColumnIdx;
        var v17 = v16 >= 0 ? row.ReadColumn<sbyte>(v16) : ~v16;
        var v18 = columnOffset + ArticleColumnIdx;
        var v19 = v18 >= 0 ? row.ReadColumn<sbyte>(v18) : ~v18;
        var v20 = 4 * (v33 + 6 + 2 * v15);

        if (v19 != 0)
        {
            var v21 = attributiveSheet.GetRow(Person)?.ReadColumn<LuminaSeString>(v20);
            if (v21 != null)
                output->SetString(v21.RawData.WithNullTerminator());

            // skipping link marker ("//")

            var v30 = row.ReadColumn<LuminaSeString>(columnOffset + textColumnIdx);
            if (v30 != null)
            {
                temp->SetString(v30.RawData.WithNullTerminator());
                output->Append(temp);
            }

            if (Amount <= 1)
            {
                var v31 = Amount.ToString();
                placeholder->SetString("[n]");
                temp->SetString(v31);
                Statics.Utf8StringReplace(output, placeholder, temp);
            }
        }
        else
        {
            if (v17 != 0 && (Amount > 1 || v17 == 2))
            {
                var v29 = attributiveSheet.GetRow(Person)?.ReadColumn<LuminaSeString>(v20 + 2);
                if (v29 != null)
                {
                    output->SetString(v29.RawData.WithNullTerminator());

                    // skipping link marker ("//")

                    var v30 = row.ReadColumn<LuminaSeString>(columnOffset + PluralColumnIdx);
                    if (v30 != null)
                    {
                        temp->SetString(v30.RawData.WithNullTerminator());
                        output->Append(temp);
                    }
                }
            }
            else
            {
                var v27 = attributiveSheet.GetRow(Person)?.ReadColumn<LuminaSeString>(v20 + (v17 != 0 ? 1 : 3));
                if (v27 != null)
                {
                    output->SetString(v27.RawData.WithNullTerminator());
                }

                // skipping link marker ("//")

                var v30 = row.ReadColumn<LuminaSeString>(columnOffset + SingularColumnIdx);
                if (v30 != null)
                {
                    temp->SetString(v30.RawData.WithNullTerminator());
                    output->Append(temp);
                }
            }

            var v31 = Amount.ToString();
            placeholder->SetString("[n]");
            temp->SetString(v31);
            Statics.Utf8StringReplace(output, placeholder, temp);
        }

        return output;
    }
}
