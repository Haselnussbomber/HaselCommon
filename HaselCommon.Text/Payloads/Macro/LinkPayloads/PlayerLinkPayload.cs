using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

public class PlayerLinkPayload : LinkPayload
{
    public PlayerLinkPayload() : base(LinkType.Player)
    {
    }

    // TODO: arg2 = flags, arg4 = bool
    // see inside "E8 ?? ?? ?? ?? 41 83 FF 0C"
    public PlayerLinkPayload(PlayerLinkFlag flags, uint serverId, string playerName) : base(LinkType.Player, (uint)flags, serverId, 0, playerName)
    {
    }

    public PlayerLinkFlag Flags
    {
        get => (PlayerLinkFlag)(Arg2?.ResolveNumber() ?? 0);
        set => Arg2 = new IntegerExpression((uint)value);
    }

    public uint ServerId
    {
        get => (uint)(Arg3?.ResolveNumber() ?? 0);
        set => Arg3 = new IntegerExpression(value);
    }

    public string PlayerName
    {
        get => Arg5?.ResolveString().ToString() ?? "";
        set => Arg5 = new StringExpression(new(value));
    }
}

public enum PlayerLinkFlag
{
    None = 0,
    // InCrossWorldDuty = 0x10 ?
}
