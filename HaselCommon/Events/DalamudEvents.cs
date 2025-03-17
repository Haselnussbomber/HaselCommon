using Dalamud.Game;
using System.Globalization;

namespace HaselCommon.Events;

public static class DalamudEvents
{
    public const string LanguageChanged = $"{nameof(DalamudEvents)}.LanguageChanged";

    public struct LanguageChangedArgs
    {
        public string LanguageCode;
        public CultureInfo CultureInfo;
        public ClientLanguage ClientLanguage;
        public override string ToString() => $"LanguageChangedArgs {{ LanguageCode = \"{LanguageCode}\", ClientLanguage = {ClientLanguage} }}";
    }
}
