namespace HaselCommon.Text.Payloads.Macro;

// TODO: unsure about args
public class PartyFinderLinkPayload : LinkPayload
{
    public PartyFinderLinkPayload() : base(LinkType.PartyFinder)
    {
    }

    public PartyFinderLinkPayload(uint listingId) : base(LinkType.PartyFinder, listingId)
    {
    }

    // TODO: is there more to this?
    // "0F 84 ?? ?? ?? ?? 4C 8B 88" <- creates fixed macro from AgentChatLog prepared link data
    // "89 4C 24 28 48 8D 4D 40" <- creates link macro (this payload) from fixed macro
    public PartyFinderLinkFlag Flags
    {
        get => (PartyFinderLinkFlag)((Arg4?.ResolveNumber() ?? 0) >> 0x10);
        set
        {
            var cur = (uint)(Arg4?.ResolveNumber() ?? 0) & 0xFFFF;
            Arg4 = (uint)(cur + ((byte)value << 0x10));
        }
    }
}

public enum PartyFinderLinkFlag : byte
{
    None = 0,
    LimitedToHomeWorld = 0x01,
}
