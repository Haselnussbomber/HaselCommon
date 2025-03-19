using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Dalamud.Game;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using HaselCommon.Extensions.Strings;
using HaselCommon.Services.Evaluator;
using HaselCommon.Utils;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;
using ActionSheet = Lumina.Excel.Sheets.Action;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class TextService
{
    private readonly ILogger<TextService> _logger;
    private readonly LanguageProvider _languageProvider;
    private readonly ExcelService _excelService;
    private readonly SeStringEvaluator _seStringEvaluator;

    private readonly Dictionary<string, Dictionary<string, string>> _translations = [];
    private readonly Dictionary<(Type, uint, ClientLanguage), string> _rowNameCache = [];

    [AutoPostConstruct]
    public void Initialize(PluginAssemblyProvider pluginAssemblyProvider, IDalamudPluginInterface pluginInterface)
    {
        LoadEmbeddedResource(GetType().Assembly, "HaselCommon.Translations.json");
        LoadEmbeddedResource(pluginAssemblyProvider.Assembly, $"{pluginInterface.InternalName}.Translations.json");
    }

    public void LoadEmbeddedResource(Assembly assembly, string filename)
    {
        using var stream = assembly.GetManifestResourceStream(filename);
        if (stream == null)
        {
            _logger.LogWarning("[TranslationManager] Could not find translations resource {filename} in assembly {assemblyName}", filename, assembly.ToString());
            return;
        }

        LoadDictionary(JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream) ?? []);
    }

    public void LoadDictionary(Dictionary<string, Dictionary<string, string>> translations)
    {
        foreach (var key in translations.Keys)
            _translations.Add(key, translations[key]);
    }

    public bool TryGetTranslation(string key, [MaybeNullWhen(false)] out string text)
    {
        text = default;
        return _translations.TryGetValue(key, out var entry) && (entry.TryGetValue(_languageProvider.LanguageCode, out text) || entry.TryGetValue("en", out text));
    }

    public string Translate(string key)
        => TryGetTranslation(key, out var text) ? text : key;

    public string Translate(string key, params object?[] args)
        => TryGetTranslation(key, out var text) ? string.Format(_languageProvider.CultureInfo, text, args) : key;

    public ReadOnlySeString TranslateSeString(string key, params SeStringParameter[] args)
    {
        if (!TryGetTranslation(key, out var format))
            return ReadOnlySeString.FromText(key);

        var sb = SeStringBuilder.SharedPool.Get();
        var placeholders = format.Split(['{', '}']);

        for (var i = 0; i < placeholders.Length; i++)
        {
            if (i % 2 == 1) // odd indices contain placeholders
            {
                if (int.TryParse(placeholders[i], out var placeholderIndex))
                {
                    if (placeholderIndex < args.Length)
                    {
                        var arg = args[placeholderIndex];
                        if (arg.IsString)
                        {
                            sb.Append(arg.StringValue);
                        }
                        else
                        {
                            sb.Append(arg.UIntValue);
                        }
                    }
                    else
                    {
                        sb.Append($"{placeholderIndex}"); // fallback
                    }
                }
            }
            else
            {
                sb.Append(placeholders[i]);
            }
        }

        var ross = sb.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(sb);
        return ross;
    }

    public string GetAddonText(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Addon>(id, language, (row) => row.Text);

    public string GetLobbyText(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Lobby>(id, language, (row) => row.Text);

    public string GetItemName(ExcelRowId<Item> itemId, ClientLanguage? language = null)
        => itemId.IsEventItem()
            ? GetOrCreateCachedText<EventItem>(itemId, language, (row) => row.Name)
            : GetOrCreateCachedText<Item>(itemId.GetBaseId(), language, (row) => row.Name);

    public string GetQuestName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Quest>(id, language, (row) => row.Name);

    public string GetLeveName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Leve>(id, language, (row) => row.Name);

    public string GetTraitName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Trait>(id, language, (row) => row.Name);

    public string GetActionName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<ActionSheet>(id, language, (row) => row.Name);

    public string GetEmoteName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Emote>(id, language, (row) => row.Name);

    public string GetEventActionName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<EventAction>(id, language, (row) => row.Name);

    public string GetGeneralActionName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<GeneralAction>(id, language, (row) => row.Name);

    public string GetBuddyActionName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<BuddyAction>(id, language, (row) => row.Name);

    public string GetMainCommandName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<MainCommand>(id, language, (row) => row.Name);

    public string GetCraftActionName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<CraftAction>(id, language, (row) => row.Name);

    public string GetPetActionName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<PetAction>(id, language, (row) => row.Name);

    public string GetCompanyActionName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<CompanyAction>(id, language, (row) => row.Name);

    public string GetMarkerName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Marker>(id, language, (row) => row.Name);

    public string GetFieldMarkerName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<FieldMarker>(id, language, (row) => row.Name);

    public string GetChocoboRaceAbilityName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<ChocoboRaceAbility>(id, language, (row) => row.Name);

    public string GetChocoboRaceItemName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<ChocoboRaceItem>(id, language, (row) => row.Name);

    public string GetExtraCommandName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<ExtraCommand>(id, language, (row) => row.Name);

    public string GetQuickChatName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<QuickChat>(id, language, (row) => row.NameAction);

    public string GetActionComboRouteName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<ActionComboRoute>(id, language, (row) => row.Name);

    public string GetBgcArmyActionName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<BgcArmyAction>(id, language, (row) => row.Unknown0);

    public string GetPerformanceInstrumentName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Perform>(id, language, (row) => row.Instrument);

    public string GetMcGuffinName(uint id, ClientLanguage? language = null)
    {
        return GetOrCreateCachedText<McGuffin>(id, language, GetMcGuffinUIName);

        ReadOnlySeString GetMcGuffinUIName(McGuffin mcGuffinRow)
            => _excelService.TryGetRow<McGuffinUIData>(mcGuffinRow.UIData.RowId, out var mcGuffinUIDataRow)
                ? mcGuffinUIDataRow.Name
                : default;
    }

    public string GetGlassesName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Glasses>(id, language, (row) => row.Name);

    public string GetOrnamentName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Ornament>(id, language, (row) => row.Singular);

    public string GetMountName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Mount>(id, language, (row) => row.Singular);

    public string GetPlaceName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<PlaceName>(id, language, (row) => row.Name);

    public string GetFateName(uint id, ClientLanguage? language = null)
        => GetOrCreateCachedText<Fate>(id, language, (row) => _seStringEvaluator.Evaluate(row.Name));

    public string GetBNpcName(uint id, ClientLanguage? language = null)
        => FromObjStr(ObjectKind.BattleNpc, id, language);

    public string GetENpcResidentName(uint id, ClientLanguage? language = null)
        => FromObjStr(ObjectKind.EventNpc, id, language);

    public string GetTreasureName(uint id, ClientLanguage? language = null)
        => FromObjStr(ObjectKind.Treasure, id, language);

    public string GetGatheringPointName(uint id, ClientLanguage? language = null)
        => FromObjStr(ObjectKind.GatheringPoint, id, language);

    public string GetEObjName(uint id, ClientLanguage? language = null)
        => FromObjStr(ObjectKind.EventObj, id, language);

    public string GetCompanionName(uint id, ClientLanguage? language = null)
        => FromObjStr(ObjectKind.Companion, id, language);

    private string FromObjStr(ObjectKind objectKind, uint id, ClientLanguage? language = null)
        => _seStringEvaluator.EvaluateFromAddon(2025, [GetObjStrId(objectKind, id)], language).ExtractText().StripSoftHyphen();

    // "8D 41 FE 83 F8 0C 77 4D"
    private static uint GetObjStrId(ObjectKind objectKind, uint id)
    {
        return objectKind switch
        {
            ObjectKind.BattleNpc => id < 1000000 ? id : id - 900000,
            ObjectKind.EventNpc => id,
            ObjectKind.Treasure or
            ObjectKind.Aetheryte or
            ObjectKind.GatheringPoint or
            ObjectKind.Companion or
            ObjectKind.HousingEventObject => id + 1000000 * (uint)objectKind - 2000000,
            ObjectKind.EventObj => id + 1000000 * (uint)objectKind - 4000000,
            ObjectKind.MjiObject => id + 3000000,
            _ => 0,
        };
    }

    private string GetOrCreateCachedText<T>(uint rowId, ClientLanguage? language, Func<T, ReadOnlySeString> getText) where T : struct, IExcelRow<T>
    {
        var lang = language ?? _languageProvider.ClientLanguage;
        var key = (typeof(T), rowId, lang);

        if (_rowNameCache.TryGetValue(key, out var text))
            return text;

        if (!_excelService.TryGetRow<T>(rowId, lang, out var row))
        {
            _rowNameCache.Add(key, text = $"{typeof(T).Name}#{rowId}");
            return text;
        }

        var tempText = getText(row);
        _rowNameCache.Add(key, text = tempText.IsEmpty ? $"{typeof(T).Name}#{rowId}" : tempText.ExtractText().StripSoftHyphen());
        return text;
    }
}
