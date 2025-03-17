using HaselCommon.Services;

namespace HaselCommon.Events;

[RegisterSingleton<IEventProvider>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
internal sealed unsafe partial class DalamudEventsProvider : IEventProvider
{
    private readonly EventDispatcher _eventDispatcher;
    private readonly LanguageProvider _languageProvider;

    [AutoPostConstruct]
    private void Initialize()
    {
        _languageProvider.LanguageChanged += OnLanguageChanged;
    }

    public void Dispose()
    {
        _languageProvider.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(string langCode)
    {
        _eventDispatcher.Trigger(DalamudEvents.LanguageChanged, new DalamudEvents.LanguageChangedArgs
        {
            LanguageCode = _languageProvider.LanguageCode,
            ClientLanguage = _languageProvider.ClientLanguage,
            CultureInfo = _languageProvider.CultureInfo,
        });
    }
}
