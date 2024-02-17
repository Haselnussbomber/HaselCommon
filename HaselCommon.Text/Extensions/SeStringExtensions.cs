using Dalamud.Game.Text.SeStringHandling;

namespace HaselCommon.Text.Extensions;

public static class SeStringExtensions
{
    public static HaselSeString Resolve(this SeString str, List<HaselSeString>? localParameterData = null)
        => HaselSeString.Parse(str.Encode()).Resolve(localParameterData);

    public static HaselSeString Resolve(this Lumina.Text.SeString str, List<HaselSeString>? localParameterData = null)
        => HaselSeString.Parse(str.RawData).Resolve(localParameterData);
}
