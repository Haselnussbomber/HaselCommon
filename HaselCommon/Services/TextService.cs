using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using HaselCommon.Extensions;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using ActionSheet = Lumina.Excel.GeneratedSheets.Action;

namespace HaselCommon.Services;

public class TextService : IDisposable
{
    private readonly ExcelService ExcelService;
    private readonly TranslationManager TranslationManager;
    private readonly TextDecoder TextDecoder;

    public event IDalamudPluginInterface.LanguageChangedDelegate? LanguageChanged;

    public TextService(ExcelService excelService, TranslationManager translationManager, TextDecoder textDecoder)
    {
        ExcelService = excelService;
        TranslationManager = translationManager;
        TextDecoder = textDecoder;

        TranslationManager.LanguageChanged += OnLanguageChanged;
    }

    public void Dispose()
    {
        TranslationManager.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(string langCode)
    {
        LanguageChanged?.Invoke(langCode);
    }

    // [GeneratedRegex("^[\\ue000-\\uf8ff]+ ")]
    // private partial Regex Utf8PrivateUseAreaRegex();

    public string Translate(string key)
        => TranslationManager.Translate(key);

    public string Translate(string key, params object?[] args)
        => TranslationManager.Translate(key, args);

    public SeString TranslateSe(string key, params SeString[] args)
        => TranslationManager.TranslateSeString(key, args.Select(s => s.Payloads).ToArray());

    public bool TryGetTranslation(string key, [MaybeNullWhen(false)] out string text)
        => TranslationManager.TryGetTranslation(key, out text);

    public void Draw(string key)
        => ImGui.TextUnformatted(Translate(key));

    public void Draw(string key, params object?[] args)
        => ImGui.TextUnformatted(Translate(key, args));

    public void Draw(HaselColor color, string key)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)color))
            ImGui.TextUnformatted(Translate(key));
    }

    public void Draw(HaselColor color, string key, params object?[] args)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)color))
            ImGui.TextUnformatted(Translate(key, args));
    }

    public void DrawWrapped(string key)
        => ImGuiHelpers.SafeTextWrapped(Translate(key));

    public void DrawWrapped(string key, params object?[] args)
        => ImGuiHelpers.SafeTextWrapped(Translate(key, args));

    public void DrawWrapped(HaselColor color, string key)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)color))
            ImGuiHelpers.SafeTextWrapped(Translate(key));
    }

    public void DrawWrapped(HaselColor color, string key, params object?[] args)
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
            return ExcelService.GetRow<EventItem>(itemId)?.Name.ExtractText() ?? $"EventItem#{itemId}";

        // HighQuality
        if (itemId is > 1_000_000 and < 2_000_000)
            itemId -= 1_000_000;

        // Collectible
        if (itemId is > 500_000 and < 1_000_000)
            itemId -= 500_000;

        return ExcelService.GetRow<Item>(itemId)?.Name.ExtractText() ?? $"Item#{itemId}";
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

    public string GetMountName(uint id)
        => TitleCasedSingularNoun("Mount", id);

    public string GetOrnamentName(uint id)
        => TitleCasedSingularNoun("Ornament", id);

    private string TitleCasedSingularNoun(string sheetName, uint id)
        => TranslationManager.CultureInfo.TextInfo.ToTitleCase(TextDecoder.ProcessNoun(TranslationManager.ClientLanguage, sheetName, 5, (int)id));
}
