namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Link)] // n n n n s
public class LinkPayload : HaselMacroPayload
{
    public LinkPayload()
    {
    }

    public LinkPayload(LinkType type, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, string arg5 = "")
    {
        Type = (uint)type;
        Arg2 = arg2;
        Arg3 = arg3;
        Arg4 = arg4;
        Arg5 = arg5;
    }

    public ExpressionWrapper? Type { get; set; }
    public ExpressionWrapper? Arg2 { get; set; }
    public ExpressionWrapper? Arg3 { get; set; }
    public ExpressionWrapper? Arg4 { get; set; }
    public ExpressionWrapper? Arg5 { get; set; }
}
