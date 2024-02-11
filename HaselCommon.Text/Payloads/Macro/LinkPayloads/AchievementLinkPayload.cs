using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

public class AchievementLinkPayload : LinkPayload
{
    public AchievementLinkPayload() : base(LinkType.Achievement)
    {
    }

    public AchievementLinkPayload(uint achievementId) : base(LinkType.Achievement, achievementId)
    {
    }

    public uint AchievementId
    {
        get => (uint)(Arg2?.ResolveNumber() ?? 0);
        set => Arg2 = new IntegerExpression(value);
    }
}
