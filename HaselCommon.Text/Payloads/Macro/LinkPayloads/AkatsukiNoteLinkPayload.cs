using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

// TODO: untested
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
        set => Arg2 = new IntegerExpression(value);
    }
}
