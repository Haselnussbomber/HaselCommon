namespace HaselCommon.Extensions;

public static class ClientLanguageExtensions
{
    extension(ClientLanguage value)
    {
        public static ClientLanguage FromCode(string langCode, ClientLanguage? fallback = null)
        {
            return langCode switch
            {
                "en" => ClientLanguage.English,
                "de" => ClientLanguage.German,
                "fr" => ClientLanguage.French,
                "ja" => ClientLanguage.Japanese,
                _ when fallback.HasValue => fallback.Value,
                _ => ClientLanguage.English
            };
        }
    }
}
