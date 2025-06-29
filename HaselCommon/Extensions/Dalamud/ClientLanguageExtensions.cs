namespace HaselCommon.Extensions;

public static class ClientLanguageExtensions
{
    extension(ClientLanguage language)
    {
        public string ToCode()
        {
            return language switch
            {
                ClientLanguage.German => "de",
                ClientLanguage.French => "fr",
                ClientLanguage.Japanese => "ja",
                _ => "en"
            };
        }
    }

    extension(string code)
    {
        public ClientLanguage ToClientlanguage()
        {
            return code switch
            {
                "de" => ClientLanguage.German,
                "fr" => ClientLanguage.French,
                "ja" => ClientLanguage.Japanese,
                _ => ClientLanguage.English
            };
        }
    }
}
