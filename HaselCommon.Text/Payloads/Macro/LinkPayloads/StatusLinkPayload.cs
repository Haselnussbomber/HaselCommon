namespace HaselCommon.Text.Payloads.Macro;

public class StatusLinkPayload : LinkPayload
{
    public StatusLinkPayload() : base(LinkType.Status)
    {
    }

    public StatusLinkPayload(uint statusId) : base(LinkType.Status, statusId, arg5: " ")
    {
    }

    public uint StatusId
    {
        get => (uint)(Arg2?.ResolveNumber() ?? 0);
        set => Arg2 = value;
    }
}
