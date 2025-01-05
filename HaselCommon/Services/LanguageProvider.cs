using System.Globalization;
using Dalamud.Game;
using Dalamud.Plugin;
using HaselCommon.Extensions.Dalamud;

namespace HaselCommon.Services;

public class LanguageProvider : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;

    public CultureInfo CultureInfo { get; private set; }
    public ClientLanguage ClientLanguage { get; private set; }
    public string LanguageCode { get; private set; }

    public event Action<string>? LanguageChanged;

    public LanguageProvider(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;

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
}
