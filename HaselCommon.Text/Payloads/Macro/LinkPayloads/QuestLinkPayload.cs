using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

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
        get => (uint)(Arg2?.ResolveNumber() ?? 0);
        set => Arg2 = new IntegerExpression(value);
    }
}
