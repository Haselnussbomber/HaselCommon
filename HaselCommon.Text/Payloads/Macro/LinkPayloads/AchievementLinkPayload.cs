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
        set => Arg2 = value;
    }
}
