namespace HaselCommon.Extensions;

public static class LanguageExtensions
{
    extension(Language value)
    {
        public ClientLanguage ToClientLanguage()
        {
            return value switch
            {
                Language.German => ClientLanguage.German,
                Language.French => ClientLanguage.French,
                Language.Japanese => ClientLanguage.Japanese,
                _ => ClientLanguage.English
            };
        }

        public string ToCode()
        {
            return value switch
            {
                Language.English => "en",
                Language.German => "de",
                Language.Japanese => "ja",
                Language.French => "fr",
                Language.Italian => "it",
                Language.Spanish => "es",
                Language.Korean => "ko",
                Language.Norwegian => "no",
                Language.Russian => "ru",
                Language.ChineseSimplified => "zh",
                Language.ChineseTraditional => "tw",
                _ => "en"
            };
        }

        public static Language FromClientLanguage(ClientLanguage clientLanguage)
        {
            return clientLanguage switch
            {
                ClientLanguage.German => Language.German,
                ClientLanguage.French => Language.French,
                ClientLanguage.Japanese => Language.Japanese,
                _ => Language.English
            };
        }

        public static Language FromCode(string langCode, bool throwError = false)
        {
            return langCode switch
            {
                "en" => Language.English,
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
                _ => throwError ? throw new ArgumentOutOfRangeException($"Language with code {langCode} not found") : Language.English
            };
        }
    }
}
