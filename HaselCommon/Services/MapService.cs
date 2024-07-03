using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Services;

public class MapService(IClientState ClientState, IGameGui GameGui)
{
    public static Vector2 GetCoords(Level level)
    {
        var map = level.Map.Value;
        var c = map!.SizeFactor / 100.0f;
        var x = 41.0f / c * (((level.X + map.OffsetX) * c + 1024.0f) / 2048.0f) + 1f;
        var y = 41.0f / c * (((level.Z + map.OffsetY) * c + 1024.0f) / 2048.0f) + 1f;
        return new(x, y);
    }

    public void OpenMap(Level level)
    {
        var map = level.Map.Value;
        if (map == null)
            return;

        var terr = map.TerritoryType.Value;
        if (terr == null)
            return;

        GameGui.OpenMapWithMapLink(new MapLinkPayload(
            terr.RowId,
            map.RowId,
            (int)(level.X * 1_000f),
            (int)(level.Z * 1_000f)
        ));
    }

    public float GetDistanceFromPlayer(Level level)
    {
        var localPlayer = ClientState.LocalPlayer;
        if (localPlayer == null || level.Territory.Row != ClientState.TerritoryType)
            return float.MaxValue; // far, far away

        return Vector2.Distance(
            new Vector2(localPlayer.Position.X, localPlayer.Position.Z),
            new Vector2(level.X, level.Z)
        );
    }
}
