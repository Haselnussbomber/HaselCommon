namespace HaselCommon.Text.Payloads.Macro;

public class HowToLinkPayload : LinkPayload
{
    public HowToLinkPayload() : base(LinkType.HowTo)
    {
    }

    public HowToLinkPayload(uint howToId) : base(LinkType.HowTo, howToId)
    {
    }

    public uint HowToId
    {
        get => (uint)(Arg2?.ResolveNumber() ?? 0);
        set => Arg2 = value;
    }
}
