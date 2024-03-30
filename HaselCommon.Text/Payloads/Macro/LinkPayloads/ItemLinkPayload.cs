namespace HaselCommon.Text.Payloads.Macro;

public class ItemLinkPayload : LinkPayload
{
    public ItemLinkPayload() : base(LinkType.Item)
    {
    }

    public ItemLinkPayload(uint itemId) : base(LinkType.Item, itemId)
    {
    }

    public uint ItemId
    {
        get => (uint)(Arg2?.ResolveNumber() ?? 0);
        set => Arg2 = value;
    }
}
