namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Link)] // n n n n s
public class LinkPayload : HaselMacroPayload
{
    public LinkPayload()
    {
    }

    public LinkPayload(LinkType type, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, string arg5 = "")
    {
        Type = new IntegerExpression((uint)type);
        Arg2 = new IntegerExpression(arg2);
        Arg3 = new IntegerExpression(arg3);
        Arg4 = new IntegerExpression(arg4);
        Arg5 = new StringExpression(new(arg5));
    }

    public BaseExpression? Type { get; set; }
    public BaseExpression? Arg2 { get; set; }
    public BaseExpression? Arg3 { get; set; }
    public BaseExpression? Arg4 { get; set; }
    public BaseExpression? Arg5 { get; set; }
}
