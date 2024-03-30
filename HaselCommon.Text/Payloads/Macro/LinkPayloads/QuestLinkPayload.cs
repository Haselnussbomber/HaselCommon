namespace HaselCommon.Text.Payloads.Macro;

public class QuestLinkPayload : LinkPayload
{
    public QuestLinkPayload() : base(LinkType.Quest)
    {
    }

    public QuestLinkPayload(uint questId) : base(LinkType.Quest, questId)
    {
    }

    public uint QuestId
    {
        get => (uint)(Arg2?.ResolveNumber() + 65536 ?? 0);
        set => Arg2 = value - 65536;
    }
}
