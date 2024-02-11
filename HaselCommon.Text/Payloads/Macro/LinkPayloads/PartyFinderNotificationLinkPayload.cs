using HaselCommon.Text.Enums;

namespace HaselCommon.Text.Payloads.Macro;

public class PartyFinderNotificationLinkPayload : LinkPayload
{
    public PartyFinderNotificationLinkPayload() : base(LinkType.PartyFinderNotification)
    {
    }
}
