namespace HaselCommon.Text.Payloads.Macro;

public class PlayerLinkPayload : LinkPayload
{
    public PlayerLinkPayload() : base(LinkType.Player)
    {
    }

    // TODO: arg2 = flags, arg4 = bool
    // see inside "E8 ?? ?? ?? ?? 41 83 FF 0C"
    public PlayerLinkPayload(PlayerLinkFlag flags, uint worldId, string playerName, bool unkBool = true) : base(LinkType.Player, (uint)flags, worldId, unkBool ? 0u : 1, playerName)
    {
    }

    public PlayerLinkFlag Flags
    {
        get => (PlayerLinkFlag)(Arg2?.ResolveNumber() ?? 0);
        set => Arg2 = (uint)value;
    }

    public uint WorldId
    {
        get => (uint)(Arg3?.ResolveNumber() ?? 0);
        set => Arg3 = value;
    }

    public string PlayerName
    {
        get => Arg5?.ResolveString().ToString() ?? "";
        set => Arg5 = value;
    }
}

// a couple here: "45 1B F6 41 83 E6 0F"
public enum PlayerLinkFlag
{
    None = 0,
    // Novice Network start
    NewAdventurer = 1,
    Returner = 2,
    Mentor = 3,
    BattleMentor = 4,
    TradeMentor = 5,
    PvpMentor = 6,
    ExpiredMentor = 7,
    // Unk = 8, // "FF CD 83 FD 08" ??
    // Novice Network end

    // IsSameHomeWorld = 10, ?
    // 11,
    // IsCrossWorld = 12, ?
    // 13,
    // InCrossWorldDuty = 0x10, ? controls context menu entry "Add to Blacklist"
    // IsGameMaster = 0x1000,
}
