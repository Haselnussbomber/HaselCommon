namespace HaselCommon.Text.Extensions;

public static class SeStringExtensions
{
    public static SeString Resolve(this Dalamud.Game.Text.SeStringHandling.SeString str, List<Expression>? localParameters = null)
        => SeString.Parse(str.Encode()).Resolve(localParameters);

    public static SeString Resolve(this Lumina.Text.SeString str, List<Expression>? localParameters = null)
        => SeString.Parse(str.RawData).Resolve(localParameters);
}
