namespace HaselCommon.Text.Enums;

public enum LinkType
{
    Player = 0x00,

    Item = 0x02, // check arg4 "C1 E2 10 03 D0"
    MapPosition = 0x03,
    Quest = 0x04,
    Achievement = 0x05,
    HowTo = 0x06,
    PartyFinderNotification = 0x07,
    Status = 0x08,
    PartyFinder = 0x09,
    AkatsukiNote = 0x0A,

    Dalamud = 0x0E,

    LinkTerminator = 0xCE,
}
