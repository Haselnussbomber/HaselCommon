using HaselCommon.Enums;

namespace HaselCommon.Interfaces;

public interface ITranslationConfig
{
    public string PluginLanguage { get; set; }
    public PluginLanguageOverride PluginLanguageOverride { get; set; }
}
