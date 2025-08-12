using System.Globalization;

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
        SetLanguage(_pluginInterface.UiLanguage);
        _pluginInterface.LanguageChanged += PluginInterface_LanguageChanged;
    }

    public void Dispose()
    {
        _pluginInterface.LanguageChanged -= PluginInterface_LanguageChanged;
    }

    private void PluginInterface_LanguageChanged(string langCode)
    {
        SetLanguage(langCode);
        LanguageChanged?.Invoke(LanguageCode);
    }

    private void SetLanguage(string langCode)
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
        ClientLanguage = langCode.ToClientLanguage();
        CultureInfo = GetCultureInfoFromLangCode(LanguageCode);
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
}
