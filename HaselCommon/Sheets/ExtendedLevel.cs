using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedLevel : Level
{
    private static readonly Dictionary<uint, ExtendedLevel?> ObjectIdCache = [];

    public static ExtendedLevel? GetByObjectId(uint objId)
    {
        if (!ObjectIdCache.TryGetValue(objId, out var value))
        {
            value = FindRow<ExtendedLevel>(row => row?.Object == objId);
            ObjectIdCache.Add(objId, value);
        }

        return value;
    }

    public Vector2 GetCoords()
    {
        var map = Map.Value;
        var c = map!.SizeFactor / 100.0f;
        var x = 41.0f / c * (((X + map.OffsetX) * c + 1024.0f) / 2048.0f) + 1f;
        var y = 41.0f / c * (((Z + map.OffsetY) * c + 1024.0f) / 2048.0f) + 1f;
        return new(x, y);
    }

    public void OpenMap()
    {
        var map = Map.Value;
        if (map == null)
            return;

        var terr = map.TerritoryType.Value;
        if (terr == null)
            return;

        Service.GameGui.OpenMapWithMapLink(new MapLinkPayload(
            terr.RowId,
            map.RowId,
            (int)(X * 1_000f),
            (int)(Z * 1_000f)
        ));
    }

    public float GetDistanceFromPlayer()
    {
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null || Territory.Row != Service.ClientState.TerritoryType)
            return float.MaxValue; // far, far away

        return Vector2.Distance(
            new Vector2(localPlayer.Position.X, localPlayer.Position.Z),
            new Vector2(X, Z)
        );
    }
}
