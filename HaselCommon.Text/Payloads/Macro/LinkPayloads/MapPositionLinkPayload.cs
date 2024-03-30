using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Text.Payloads.Macro;

public class MapPositionLinkPayload : LinkPayload
{
    public MapPositionLinkPayload() : base(LinkType.MapPosition)
    {
    }

    // TODO: add overload with x and y as float (to be able to pass GameObject.Position) and encode it
    // TODO: what about Z and instanceId? are they only text?
    public MapPositionLinkPayload(uint territoryType, uint mapId, float x, float y) : base(LinkType.MapPosition, (territoryType << 0x10) + mapId)
    {
        Map = GetRow<Map>(mapId);
        X = x;
        Y = y;
    }

    public Map? Map { get; private set; }

    public ushort MapId
    {
        get => (ushort)((Arg2?.ResolveNumber() ?? 0) & 0xFFFF);
        set
        {
            var cur = (uint)(Arg2?.ResolveNumber() ?? 0) & 0xFFFF0000;
            Arg2 = cur + value;
            Map = GetRow<Map>(value);
        }
    }

    public ushort TerritoryTypeId
    {
        get => (ushort)((Arg2?.ResolveNumber() ?? 0) >> 0x10);
        set
        {
            var cur = (uint)(Arg2?.ResolveNumber() ?? 0) & 0xFFFF;
            Arg2 = (uint)(cur + (value << 0x10));
        }
    }

    public float X
    {
        get => (Arg3?.ResolveNumber() ?? 0) / 1000f;
        set => Arg3 = (uint)(value * 1000f);
    }

    public float Y
    {
        get => (Arg4?.ResolveNumber() ?? 0) / 1000f;
        set => Arg4 = (uint)(value * 1000f);
    }

    public float MapPosX => (Map?.ConvertRawToMapPosX(X) ?? 10) / 10f;
    public float MapPosY => (Map?.ConvertRawToMapPosY(Y) ?? 10) / 10f;
}
