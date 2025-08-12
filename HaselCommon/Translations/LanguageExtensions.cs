namespace HaselCommon;

public static class LanguageExtensions
{
    public static ClientLanguage ToClientLanguage(this Language value)
        => value switch
        {
            Language.German => ClientLanguage.German,
            Language.French => ClientLanguage.French,
            Language.Japanese => ClientLanguage.Japanese,
            _ => ClientLanguage.English
        };
}
