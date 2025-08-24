namespace HaselCommon.Extensions;

public static class ClientLanguageExtensions
{
    public static string ToCode(this ClientLanguage value)
        => value switch
        {
            ClientLanguage.German => "de",
            ClientLanguage.French => "fr",
            ClientLanguage.Japanese => "ja",
            _ => "en"
        };

    public static ClientLanguage ToClientLanguage(this string value)
        => value switch
        {
            "de" => ClientLanguage.German,
            "fr" => ClientLanguage.French,
            "ja" => ClientLanguage.Japanese,
            _ => ClientLanguage.English
        };
}
