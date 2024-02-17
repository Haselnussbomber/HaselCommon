namespace HaselCommon.Text.Payloads.Macro;

public class MapPositionLinkPayload : LinkPayload
{
    public MapPositionLinkPayload() : base(LinkType.MapPosition)
    {
    }

    // TODO: add overload with x and y as float (to be able to pass GameObject.Position) and encode it
    public MapPositionLinkPayload(uint territoryType, uint mapId, uint x, uint y) : base(LinkType.MapPosition, (territoryType << 0x10) + mapId, x, y)
    {
    }

    public ushort MapId
    {
        get => (ushort)((Arg2?.ResolveNumber() ?? 0) & 0xFFFF);
        set
        {
            var cur = (uint)(Arg2?.ResolveNumber() ?? 0) & 0xFFFF0000;
            Arg2 = new IntegerExpression(cur + value);
        }
    }

    public ushort TerritoryTypeId
    {
        get => (ushort)((Arg2?.ResolveNumber() ?? 0) >> 0x10);
        set
        {
            var cur = (uint)(Arg2?.ResolveNumber() ?? 0) & 0xFFFF;
            Arg2 = new IntegerExpression((uint)(cur + (value << 0x10)));
        }
    }
}
