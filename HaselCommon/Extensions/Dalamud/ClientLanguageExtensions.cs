using Dalamud.Game;

namespace HaselCommon.Extensions.Dalamud;

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

    public static ClientLanguage ToClientlanguage(this string value)
        => value switch
        {
            "de" => ClientLanguage.German,
            "fr" => ClientLanguage.French,
            "ja" => ClientLanguage.Japanese,
            _ => ClientLanguage.English
        };
}
