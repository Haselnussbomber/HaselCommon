using Dalamud.Game.Text.SeStringHandling;

namespace HaselCommon.Text.Extensions;

public static class SeStringExtensions
{
    public static HaselSeString Resolve(this SeString str, List<ExpressionWrapper>? localParameters = null)
        => HaselSeString.Parse(str.Encode()).Resolve(localParameters);

    public static HaselSeString Resolve(this Lumina.Text.SeString str, List<ExpressionWrapper>? localParameters = null)
        => HaselSeString.Parse(str.RawData).Resolve(localParameters);
}
