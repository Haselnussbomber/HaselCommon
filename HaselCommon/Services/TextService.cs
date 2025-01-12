using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Dalamud.Game;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using HaselCommon.Extensions.Strings;
using HaselCommon.Graphics;
using HaselCommon.Services.SeStringEvaluation;
using ImGuiNET;
using Lumina.Excel.Sheets;
using Lumina.Text;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;
using ActionSheet = Lumina.Excel.Sheets.Action;

namespace HaselCommon.Services;

[RegisterSingleton]
public class TextService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = [];
    private readonly ILogger<TextService> _logger;
    private readonly LanguageProvider _languageProvider;
    private readonly ExcelService _excelService;
    private readonly SeStringEvaluatorService _seStringEvaluator;

    public TextService(
        ILogger<TextService> logger,
        IDalamudPluginInterface pluginInterface,
        LanguageProvider languageProvider,
        PluginAssemblyProvider pluginAssemblyProvider,
        ExcelService excelService,
        SeStringEvaluatorService seStringEvaluator)
    {
        _logger = logger;
        _languageProvider = languageProvider;
        _excelService = excelService;
        _seStringEvaluator = seStringEvaluator;

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

        var sb = new SeStringBuilder();
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

    public void Draw(string key)
        => ImGui.TextUnformatted(Translate(key));

    public void Draw(string key, params object?[] args)
        => ImGui.TextUnformatted(Translate(key, args));

    public void Draw(Color color, string key)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)color))
            ImGui.TextUnformatted(Translate(key));
    }

    public void Draw(Color color, string key, params object?[] args)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)color))
            ImGui.TextUnformatted(Translate(key, args));
    }

    public void DrawWrapped(string key)
        => ImGuiHelpers.SafeTextWrapped(Translate(key));

    public void DrawWrapped(string key, params object?[] args)
        => ImGuiHelpers.SafeTextWrapped(Translate(key, args));

    public void DrawWrapped(Color color, string key)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)color))
            ImGuiHelpers.SafeTextWrapped(Translate(key));
    }

    public void DrawWrapped(Color color, string key, params object?[] args)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)color))
            ImGuiHelpers.SafeTextWrapped(Translate(key, args));
    }

    public string GetAddonText(uint id)
        => _excelService.TryGetRow<Addon>(id, out var row) ? row.Text.ExtractText().StripSoftHypen() : $"Addon#{id}";

    public string GetItemName(uint itemId, ClientLanguage? language = null)
    {
        // EventItem
        if (itemId is > 2_000_000)
            return _excelService.TryGetRow<EventItem>(itemId, language, out var eventItemRow) ? eventItemRow.Name.ExtractText().StripSoftHypen() : $"EventItem#{itemId}";

        // HighQuality
        if (itemId is > 1_000_000 and < 2_000_000)
            itemId -= 1_000_000;

        // Collectible
        if (itemId is > 500_000 and < 1_000_000)
            itemId -= 500_000;

        return _excelService.TryGetRow<Item>(itemId, language, out var itemRow) ? itemRow.Name.ExtractText().StripSoftHypen() : $"Item#{itemId}";
    }

    public string GetQuestName(uint id)
        => _excelService.TryGetRow<Quest>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"Quest#{id}";

    public string GetTraitName(uint id)
        => _excelService.TryGetRow<Trait>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"Trait#{id}";

    public string GetActionName(uint id)
        => _excelService.TryGetRow<ActionSheet>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"Action#{id}";

    public string GetEmoteName(uint id)
        => _excelService.TryGetRow<Emote>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"Emote#{id}";

    public string GetEventActionName(uint id)
        => _excelService.TryGetRow<EventAction>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"EventAction#{id}";

    public string GetGeneralActionName(uint id)
        => _excelService.TryGetRow<GeneralAction>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"GeneralAction#{id}";

    public string GetBuddyActionName(uint id)
        => _excelService.TryGetRow<BuddyAction>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"BuddyAction#{id}";

    public string GetMainCommandName(uint id)
        => _excelService.TryGetRow<MainCommand>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"MainCommand#{id}";

    public string GetCraftActionName(uint id)
        => _excelService.TryGetRow<CraftAction>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"CraftAction#{id}";

    public string GetPetActionName(uint id)
        => _excelService.TryGetRow<PetAction>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"PetAction#{id}";

    public string GetCompanyActionName(uint id)
        => _excelService.TryGetRow<CompanyAction>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"CompanyAction#{id}";

    public string GetMarkerName(uint id)
        => _excelService.TryGetRow<Marker>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"Marker#{id}";

    public string GetFieldMarkerName(uint id)
        => _excelService.TryGetRow<FieldMarker>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"FieldMarker#{id}";

    public string GetChocoboRaceAbilityName(uint id)
        => _excelService.TryGetRow<ChocoboRaceAbility>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"ChocoboRaceAbility#{id}";

    public string GetChocoboRaceItemName(uint id)
        => _excelService.TryGetRow<ChocoboRaceItem>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"ChocoboRaceItem#{id}";

    public string GetExtraCommandName(uint id)
        => _excelService.TryGetRow<ExtraCommand>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"ExtraCommand#{id}";

    public string GetQuickChatName(uint id)
        => _excelService.TryGetRow<QuickChat>(id, out var row) ? row.NameAction.ExtractText().StripSoftHypen() : $"QuickChat#{id}";

    public string GetActionComboRouteName(uint id)
        => _excelService.TryGetRow<ActionComboRoute>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"ActionComboRoute#{id}";

    public string GetBgcArmyActionName(uint id)
        => _excelService.TryGetRow<BgcArmyAction>(id, out var row) ? row.Unknown0.ExtractText().StripSoftHypen() : $"BgcArmyAction#{id}";

    public string GetPerformanceInstrumentName(uint id)
        => _excelService.TryGetRow<Perform>(id, out var row) ? row.Instrument.ExtractText().StripSoftHypen() : $"Perform#{id}";

    public string GetMcGuffinName(uint id)
    {
        if (!_excelService.TryGetRow<McGuffin>(id, out var mcGuffinRow))
            return $"McGuffin#{id}";

        return _excelService.TryGetRow<McGuffinUIData>(mcGuffinRow.UIData.RowId, out var mcGuffinUIDataRow) ? mcGuffinUIDataRow.Name.ExtractText().StripSoftHypen() : $"McGuffin#{id}";
    }

    public string GetGlassesName(uint id)
        => _excelService.TryGetRow<Glasses>(id, out var row) ? row.Name.ExtractText().StripSoftHypen() : $"Glasses#{id}";

    public string GetOrnamentName(uint id)
        => _excelService.TryGetRow<Ornament>(id, out var row) ? row.Singular.ExtractText().StripSoftHypen() : $"Ornament#{id}";

    public string GetMountName(uint id)
        => _excelService.TryGetRow<Mount>(id, out var row) ? row.Singular.ExtractText().StripSoftHypen() : $"Mount#{id}";

    public string GetBNpcName(uint id)
        => FromObjStr(ObjectKind.BattleNpc, id);

    public string GetENpcResidentName(uint id)
        => FromObjStr(ObjectKind.EventNpc, id);

    public string GetTreasureName(uint id)
        => FromObjStr(ObjectKind.Treasure, id);

    public string GetGatheringPointName(uint id)
        => FromObjStr(ObjectKind.GatheringPoint, id);

    public string GetEObjName(uint id)
        => FromObjStr(ObjectKind.EventObj, id);

    public string GetCompanionName(uint id)
        => FromObjStr(ObjectKind.Companion, id);

    private string FromObjStr(ObjectKind objectKind, uint id)
    {
        var objStrId = GetObjStrId(objectKind, id);

        return _seStringEvaluator.EvaluateFromAddon(2025, new SeStringContext()
        {
            Language = _languageProvider.ClientLanguage,
            LocalParameters = [objStrId]
        }).ExtractText().StripSoftHypen();
    }

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
}
