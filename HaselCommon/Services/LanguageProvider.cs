using System.Globalization;
using Dalamud;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class LanguageProvider : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;

    public CultureInfo CultureInfo { get; private set; }
    public ClientLanguage ClientLanguage { get; private set; }
    public Language Language { get; private set; }
    public string LanguageCode { get; private set; }

    public event Action<string>? LanguageChanged;

    [AutoPostConstruct]
    private void Initialize()
    {
        SetLangCode(_pluginInterface.UiLanguage);
        _pluginInterface.LanguageChanged += OnLanguageChanged;
    }

    public void Dispose()
    {
        _pluginInterface.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(string langCode)
    {
        SetLangCode(langCode);
        LanguageChanged?.Invoke(langCode);
    }

    private void SetLangCode(string langCode)
    {
        Language = langCode switch
        {
            "de" => Language.German,  
            "ja" => Language.Japanese,
            "fr" => Language.French,  
            "it" => Language.Italian, 
            "es" => Language.Spanish, 
            "ko" => Language.Korean,  
            "no" => Language.Norwegian,
            "ru" => Language.Russian, 
            "zh" => Language.ChineseSimplified,
            "tw" => Language.ChineseTraditional,
            _ => Language.English
        };

        LanguageCode = langCode;
        ClientLanguage = langCode.ToClientlanguage();
        CultureInfo = Localization.GetCultureInfoFromLangCode(langCode);
    }
}
