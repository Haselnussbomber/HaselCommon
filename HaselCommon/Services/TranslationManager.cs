using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;

namespace HaselCommon.Services;

// TODO: merge with TextService?!
public class TranslationManager : IDisposable
{
    private readonly Dictionary<string, Dictionary<string, string>> Translations = [];
    private readonly DalamudPluginInterface PluginInterface;
    private readonly IPluginLog PluginLog;

    public CultureInfo CultureInfo { get; private set; } = new("en");
    public ClientLanguage ClientLanguage { get; private set; } = ClientLanguage.English;
    public string LanguageCode { get; private set; } = "en";

    public event IDalamudPluginInterface.LanguageChangedDelegate? LanguageChanged;

    public TranslationManager(
        DalamudPluginInterface pluginInterface,
        IPluginLog pluginLog)
    {
        PluginInterface = pluginInterface;
        PluginLog = pluginLog;

        LoadEmbeddedResource(GetType().Assembly, "HaselCommon.Translations.json");
        LoadEmbeddedResource(Service.PluginAssembly, $"{PluginInterface.InternalName}.Translations.json");

        LanguageCode = PluginInterface.UiLanguage;
        ClientLanguage = PluginInterface.UiLanguage.ToClientlanguage();
        CultureInfo = GetCultureInfoFromLangCode(LanguageCode);

        PluginInterface.LanguageChanged += PluginInterface_LanguageChanged;
    }

    public void Dispose()
    {
        PluginInterface.LanguageChanged -= PluginInterface_LanguageChanged;
        GC.SuppressFinalize(this);
    }

    public void LoadEmbeddedResource(Assembly assembly, string filename)
    {
        using var stream = assembly.GetManifestResourceStream(filename);
        if (stream == null)
        {
            PluginLog.Warning("[TranslationManager] Could not find translations resource {filename} in assembly {assemblyName}", filename, assembly.ToString());
            return;
        }

        LoadDictionary(JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream) ?? []);
    }

    public void LoadDictionary(Dictionary<string, Dictionary<string, string>> translations)
    {
        foreach (var key in translations.Keys)
            Translations.Add(key, translations[key]);
    }

    private void PluginInterface_LanguageChanged(string langCode)
    {
        LanguageCode = PluginInterface.UiLanguage;
        ClientLanguage = PluginInterface.UiLanguage.ToClientlanguage();
        CultureInfo = GetCultureInfoFromLangCode(LanguageCode);

        LanguageChanged?.Invoke(langCode);
    }

    /// copied from <see cref="Dalamud.Localization.GetCultureInfoFromLangCode"/>
    private static CultureInfo GetCultureInfoFromLangCode(string langCode) =>
        CultureInfo.GetCultureInfo(langCode switch
        {
            "tw" => "zh-hant",
            "zh" => "zh-hans",
            _ => langCode,
        });

    public bool TryGetTranslation(string key, [MaybeNullWhen(false)] out string text)
    {
        text = default;
        return Translations.TryGetValue(key, out var entry) && (entry.TryGetValue(LanguageCode, out text) || entry.TryGetValue("en", out text));
    }

    public string Translate(string key)
        => TryGetTranslation(key, out var text) ? text : key;

    public string Translate(string key, params object?[] args)
        => TryGetTranslation(key, out var text) ? string.Format(CultureInfo, text, args) : key;

    public SeString TranslateSeString(string key, params IEnumerable<Payload>[] args)
    {
        if (!TryGetTranslation(key, out var format))
            return key;

        var placeholders = format.Split(['{', '}']);
        var sb = new SeStringBuilder(); // TODO: switch to Luminas SeStringBuilder

        for (var i = 0; i < placeholders.Length; i++)
        {
            if (i % 2 == 1) // odd indices contain placeholders
            {
                if (int.TryParse(placeholders[i], out var placeholderIndex))
                {
                    if (placeholderIndex < args.Length)
                    {
                        sb.BuiltString.Payloads.AddRange(args[placeholderIndex]);
                    }
                    else
                    {
                        sb.AddText($"{placeholderIndex}"); // fallback
                    }
                }
            }
            else
            {
                sb.AddText(placeholders[i]);
            }
        }

        return sb.Build();
    }
}
