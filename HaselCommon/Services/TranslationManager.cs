using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Dalamud;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Enums;
using HaselCommon.Extensions;
using HaselCommon.Interfaces;

namespace HaselCommon.Services;

public class TranslationManager : IDisposable
{
    public static readonly string DefaultLanguage = "en";
    public static readonly Dictionary<string, string> AllowedLanguages = new()
    {
        ["de"] = "German",
        ["en"] = "English",
        ["fr"] = "French",
        ["ja"] = "Japanese"
    };

    private DalamudPluginInterface _pluginInterface;
    private IClientState _clientState;

    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private ITranslationConfig _config = null!;

    private string _activeLanguage = DefaultLanguage;

    public delegate void LanguageChangedDelegate();
    public event LanguageChangedDelegate? OnLanguageChange;

    public CultureInfo CultureInfo { get; private set; } = new(DefaultLanguage);

    public TranslationManager(DalamudPluginInterface pluginInterface, IClientState clientState)
    {
        _pluginInterface = pluginInterface;
        _clientState = clientState;

        using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("HaselCommon.Translations.json")
            ?? throw new Exception($"Could not find translations resource \"HaselCommon.Translations.json\".");

        var translations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream) ?? new();

        foreach (var key in translations.Keys)
        {
            _translations.Add(key, translations[key]);
        }
    }

    public void Dispose()
    {
        _pluginInterface.LanguageChanged -= PluginInterface_LanguageChanged;
        _pluginInterface = null!;
        _clientState = null!;
        _config = null!;
    }

    public void Initialize(ITranslationConfig config, Dictionary<string, Dictionary<string, string>> translations)
    {
        _config = config;

        foreach (var key in translations.Keys)
        {
            _translations.Add(key, translations[key]);
        }

        SetLanguage(_config.PluginLanguageOverride, _config.PluginLanguage, false);

        _pluginInterface.LanguageChanged += PluginInterface_LanguageChanged;
    }

    public void Initialize(ITranslationConfig config, string? filename = null)
    {
        if (string.IsNullOrEmpty(filename))
            filename = $"{Service.PluginInterface.InternalName}.Translations.json";

        using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(filename)
            ?? throw new Exception($"Could not find translations resource \"{filename}\".");

        var translations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream) ?? new();

        Initialize(config, translations);
    }

    public PluginLanguageOverride Override
    {
        get => _config.PluginLanguageOverride;
        set => SetLanguage(value, _config.PluginLanguage);
    }

    public string Language
    {
        get => _activeLanguage;
        set => SetLanguage(_config.PluginLanguageOverride, value);
    }

    public ClientLanguage ClientLanguage => _activeLanguage.ToClientlanguage();

    private void PluginInterface_LanguageChanged(string langCode)
    {
        if (Override == PluginLanguageOverride.Dalamud)
            Language = langCode;
    }

    public void SetLanguage(PluginLanguageOverride pluginLanguageOverride, string code, bool fireOnLanguageChangeEvent = true)
    {
        code = pluginLanguageOverride switch
        {
            PluginLanguageOverride.Dalamud => _pluginInterface.UiLanguage,
            PluginLanguageOverride.Client => _clientState.ClientLanguage.ToCode(),
            _ => code,
        };

        if (!AllowedLanguages.ContainsKey(code))
            code = DefaultLanguage;

        _config.PluginLanguageOverride = pluginLanguageOverride;
        _config.PluginLanguage = code;

        if (_activeLanguage != code)
        {
            _activeLanguage = code;
            CultureInfo = new(code);
            Service.StringManager.Clear();

            if (fireOnLanguageChangeEvent)
                OnLanguageChange?.Invoke();
        }
    }

    public void UpdateLanguage()
    {
        SetLanguage(Override, Language);
    }

    public bool TryGetTranslation(string key, [MaybeNullWhen(false)] out string text)
    {
        text = default;
        return _translations.TryGetValue(key, out var entry) && (entry.TryGetValue(_activeLanguage, out text) || entry.TryGetValue("en", out text));
    }

    public string Translate(string key)
        => TryGetTranslation(key, out var text) ? text : key;

    public string Translate(string key, params object?[] args)
        => TryGetTranslation(key, out var text) ? string.Format(CultureInfo, text, args) : key;

    public SeString TranslateSeString(string key, params IEnumerable<Payload>[] args)
    {
        if (!TryGetTranslation(key, out var format))
            return key;

        var placeholders = format.Split(new[] { '{', '}' });
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
