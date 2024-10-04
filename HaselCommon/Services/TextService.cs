using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Dalamud.Game;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using HaselCommon.Extensions.Dalamud;
using HaselCommon.Extensions.Strings;
using HaselCommon.Graphics;
using HaselCommon.Services.SeStringEvaluation;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;
using ActionSheet = Lumina.Excel.GeneratedSheets.Action;

namespace HaselCommon.Services;

public class TextService : IDisposable
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = [];
    private readonly ILogger<TextService> _logger;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly TextDecoder _textDecoder;

    private ExcelService? _excelService;

    public CultureInfo CultureInfo { get; private set; }
    public ClientLanguage ClientLanguage { get; private set; }
    public string LanguageCode { get; private set; }

    public event Action<string>? LanguageChanged;

    private ExcelService ExcelService => _excelService ??= Service.Get<ExcelService>();

    public TextService(ILogger<TextService> logger, IDalamudPluginInterface pluginInterface, TextDecoder textDecoder)
    {
        _logger = logger;
        _pluginInterface = pluginInterface;
        _textDecoder = textDecoder;

        LoadEmbeddedResource(GetType().Assembly, "HaselCommon.Translations.json");
        LoadEmbeddedResource(Service.PluginAssembly, $"{_pluginInterface.InternalName}.Translations.json");

        LanguageCode = _pluginInterface.UiLanguage;
        ClientLanguage = _pluginInterface.UiLanguage.ToClientlanguage();
        CultureInfo = GetCultureInfoFromLangCode(LanguageCode);

        _pluginInterface.LanguageChanged += PluginInterface_LanguageChanged;
    }

    public void Dispose()
    {
        _pluginInterface.LanguageChanged -= PluginInterface_LanguageChanged;
        GC.SuppressFinalize(this);
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

    private void PluginInterface_LanguageChanged(string langCode)
    {
        LanguageCode = _pluginInterface.UiLanguage;
        ClientLanguage = _pluginInterface.UiLanguage.ToClientlanguage();
        CultureInfo = GetCultureInfoFromLangCode(LanguageCode);
        LanguageChanged?.Invoke(LanguageCode);
    }

    /// copied from <see cref="Dalamud.Localization.GetCultureInfoFromLangCode"/>
    public static CultureInfo GetCultureInfoFromLangCode(string langCode)
    {
        return CultureInfo.GetCultureInfo(langCode switch
        {
            "tw" => "zh-hant",
            "zh" => "zh-hans",
            _ => langCode,
        });
    }

    public bool TryGetTranslation(string key, [MaybeNullWhen(false)] out string text)
    {
        text = default;
        return _translations.TryGetValue(key, out var entry) && (entry.TryGetValue(LanguageCode, out text) || entry.TryGetValue("en", out text));
    }

    public string Translate(string key)
        => TryGetTranslation(key, out var text) ? text : key;

    public string Translate(string key, params object?[] args)
        => TryGetTranslation(key, out var text) ? string.Format(CultureInfo, text, args) : key;

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

        return sb.ToReadOnlySeString();
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
        => ExcelService.GetRow<Addon>(id)?.Text.ExtractText() ?? $"Addon#{id}";

    public string GetItemName(uint itemId, ClientLanguage? language = null)
    {
        // EventItem
        if (itemId is > 2_000_000)
            return ExcelService.GetRow<EventItem>(itemId, language)?.Name.ExtractText() ?? $"EventItem#{itemId}";

        // HighQuality
        if (itemId is > 1_000_000 and < 2_000_000)
            itemId -= 1_000_000;

        // Collectible
        if (itemId is > 500_000 and < 1_000_000)
            itemId -= 500_000;

        return ExcelService.GetRow<Item>(itemId, language)?.Name.ExtractText() ?? $"Item#{itemId}";
    }

    public string GetQuestName(uint id)
        => ExcelService.GetRow<Quest>(id)?.Name.ExtractText() ?? $"Quest#{id}";

    public string GetBNpcName(uint id)
        => TitleCasedSingularNoun("BNpcName", id);

    public string GetENpcResidentName(uint id)
        => TitleCasedSingularNoun("ENpcResident", id);

    public string GetTreasureName(uint id)
        => TitleCasedSingularNoun("Treasure", id);

    public string GetGatheringPointName(uint id)
        => TitleCasedSingularNoun("GatheringPointName", id);

    public string GetEObjName(uint id)
        => TitleCasedSingularNoun("EObjName", id);

    public string GetCompanionName(uint id)
        => TitleCasedSingularNoun("Companion", id);

    public string GetTraitName(uint id)
        => ExcelService.GetRow<Trait>(id)?.Name.ExtractText() ?? $"Trait#{id}";

    public string GetActionName(uint id)
        => ExcelService.GetRow<ActionSheet>(id)?.Name.ExtractText() ?? $"Action#{id}";

    public string GetEmoteName(uint id)
        => ExcelService.GetRow<Emote>(id)?.Name.ExtractText() ?? $"Emote#{id}";

    public string GetEventActionName(uint id)
        => ExcelService.GetRow<EventAction>(id)?.Name.ExtractText() ?? $"EventAction#{id}";

    public string GetGeneralActionName(uint id)
        => ExcelService.GetRow<GeneralAction>(id)?.Name.ExtractText() ?? $"GeneralAction#{id}";

    public string GetBuddyActionName(uint id)
        => ExcelService.GetRow<BuddyAction>(id)?.Name.ExtractText() ?? $"BuddyAction#{id}";

    public string GetMainCommandName(uint id)
        => ExcelService.GetRow<MainCommand>(id)?.Name.ExtractText() ?? $"MainCommand#{id}";

    public string GetCraftActionName(uint id)
        => ExcelService.GetRow<CraftAction>(id)?.Name.ExtractText() ?? $"CraftAction#{id}";

    public string GetPetActionName(uint id)
        => ExcelService.GetRow<PetAction>(id)?.Name.ExtractText() ?? $"PetAction#{id}";

    public string GetCompanyActionName(uint id)
        => ExcelService.GetRow<CompanyAction>(id)?.Name.ExtractText() ?? $"CompanyAction#{id}";

    public string GetMarkerName(uint id)
        => ExcelService.GetRow<Marker>(id)?.Name.ExtractText() ?? $"Marker#{id}";

    public string GetFieldMarkerName(uint id)
        => ExcelService.GetRow<FieldMarker>(id)?.Name.ExtractText() ?? $"FieldMarker#{id}";

    public string GetChocoboRaceAbilityName(uint id)
        => ExcelService.GetRow<ChocoboRaceAbility>(id)?.Name.ExtractText() ?? $"ChocoboRaceAbility#{id}";

    public string GetChocoboRaceItemName(uint id)
        => ExcelService.GetRow<ChocoboRaceItem>(id)?.Name.ExtractText() ?? $"ChocoboRaceItem#{id}";

    public string GetExtraCommandName(uint id)
        => ExcelService.GetRow<ExtraCommand>(id)?.Name.ExtractText() ?? $"ExtraCommand#{id}";

    public string GetQuickChatName(uint id)
        => ExcelService.GetRow<QuickChat>(id)?.NameAction.ExtractText() ?? $"QuickChat#{id}";

    public string GetActionComboRouteName(uint id)
        => ExcelService.GetRow<ActionComboRoute>(id)?.Name.ExtractText() ?? $"ActionComboRoute#{id}";

    public string GetBgcArmyActionName(uint id)
        => ExcelService.GetRow<Lumina.Excel.GeneratedSheets2.BgcArmyAction>(id)?.Unknown0.ExtractText() ?? $"BgcArmyAction#{id}";

    public string GetPerformanceInstrumentName(uint id)
        => ExcelService.GetRow<Perform>(id)?.Instrument.ExtractText() ?? $"Perform#{id}";

    public string GetMcGuffinName(uint id)
        => ExcelService.GetRow<McGuffinUIData>(ExcelService.GetRow<McGuffin>(id)?.UIData.Row ?? 0)?.Name.ExtractText() ?? $"McGuffin#{id}";

    public string GetMountName(uint id)
        => TitleCasedSingularNoun("Mount", id);

    public string GetOrnamentName(uint id)
        => TitleCasedSingularNoun("Ornament", id);

    public string GetGlassesName(uint id)
        => TitleCasedSingularNoun("Glasses", id);

    private string TitleCasedSingularNoun(string sheetName, uint id)
        => CultureInfo.TextInfo.ToTitleCase(_textDecoder.ProcessNoun(ClientLanguage, sheetName, 5, (int)id).ExtractText());
}
