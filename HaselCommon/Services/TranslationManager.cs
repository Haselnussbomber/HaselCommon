using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Dalamud;
using Dalamud.Game.Text.SeStringHandling;
using HaselCommon.Extensions;

namespace HaselCommon.Services;

public class TranslationManager : IDisposable
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = [];

    public CultureInfo CultureInfo { get; private set; } = new("en");
    public ClientLanguage ClientLanguage { get; private set; } = ClientLanguage.English;
    public string LanguageCode { get; private set; } = "en";

    public void Initialize()
    {
        LoadEmbeddedResource(GetType().Assembly, "HaselCommon.Translations.json");
        LoadEmbeddedResource(Assembly.GetCallingAssembly(), $"{Service.PluginInterface.InternalName}.Translations.json");

        LanguageCode = Service.PluginInterface.UiLanguage;
        ClientLanguage = Service.PluginInterface.UiLanguage.ToClientlanguage();
        CultureInfo = new(LanguageCode);

        Service.PluginInterface.LanguageChanged += PluginInterface_LanguageChanged;
    }

    public void Dispose()
    {
        Service.PluginInterface.LanguageChanged -= PluginInterface_LanguageChanged;
    }

    public void LoadEmbeddedResource(Assembly assembly, string filename)
    {
        using var stream = assembly.GetManifestResourceStream(filename);
        if (stream == null)
        {
            Service.PluginLog.Warning("[TranslationManager] Could not find translations resource {filename} in assembly {assemblyName}", filename, assembly.Location);
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
        LanguageCode = Service.PluginInterface.UiLanguage;
        ClientLanguage = Service.PluginInterface.UiLanguage.ToClientlanguage();
        CultureInfo = new(LanguageCode);
        StringManager.NameCache.Clear();
        StringManager.TextCache.Clear();
        ExcelCache.Clear();
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

    public SeString TranslateSeString(string key, params IEnumerable<Payload>[] args)
    {
        if (!TryGetTranslation(key, out var format))
            return key;

        var placeholders = format.Split(['{', '}']);
        var sb = new SeStringBuilder();

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
