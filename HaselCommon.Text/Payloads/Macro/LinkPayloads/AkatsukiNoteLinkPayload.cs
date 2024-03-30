namespace HaselCommon.Text.Payloads.Macro;

public class AkatsukiNoteLinkPayload : LinkPayload
{
    public AkatsukiNoteLinkPayload() : base(LinkType.AkatsukiNote)
    {
    }

    public AkatsukiNoteLinkPayload(uint akatsukiNoteId) : base(LinkType.AkatsukiNote, akatsukiNoteId)
    {
    }

    public uint AkatsukiNoteId
    {
        get => (uint)(Arg2?.ResolveNumber() ?? 0);
        set => Arg2 = value;
    }
}
