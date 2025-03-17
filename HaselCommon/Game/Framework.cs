using Dalamud.Game;

using CSFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace HaselCommon.Game;

public static unsafe class Framework
{
    public static ClientLanguage ClientLanguage => (ClientLanguage)CSFramework.Instance()->ClientLanguage;
}
