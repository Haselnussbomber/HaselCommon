namespace HaselCommon.Extensions;

public static class ClientLanguageExtensions
{
    extension(ClientLanguage value)
    {
        public string ToCode()
        {
            return value switch
            {
                ClientLanguage.German => "de",
                ClientLanguage.French => "fr",
                ClientLanguage.Japanese => "ja",
                _ => "en"
            };
        }
    }

    extension(string value)
    {
        public ClientLanguage ToClientlanguage()
        {
            return value switch
            {
                "de" => ClientLanguage.German,
                "fr" => ClientLanguage.French,
                "ja" => ClientLanguage.Japanese,
                _ => ClientLanguage.English
            };
        }
    }
}
