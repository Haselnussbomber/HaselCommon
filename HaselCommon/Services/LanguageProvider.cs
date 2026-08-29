using System.Globalization;
using Dalamud;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class LanguageProvider : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;

    public CultureInfo CultureInfo { get; private set; }
    public ClientLanguage ClientLanguage { get; private set; }
    public string LanguageCode { get; private set; }

    public event Action LanguageChanged;

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
        LanguageChanged.InvokeSafely();
    }

    private void SetLangCode(string langCode)
    {
        LanguageCode = langCode;
        ClientLanguage = langCode.ToClientlanguage();
        CultureInfo = Localization.GetCultureInfoFromLangCode(langCode);
    }

    public IDisposable OnLanguageChange(Action handler)
    {
        LanguageChanged += handler.Invoke;
        return new DisposableAction(() => LanguageChanged -= handler.Invoke);
    }
}
