namespace HaselCommon.Commands;

[AttributeUsage(AttributeTargets.Method)]
public class CommandHandlerAttribute(string Command, string HelpMessageKey, bool ShowInHelp = true, int DisplayOrder = -1) : Attribute
{
    public string Command { get; } = Command;
    public string HelpMessageKey { get; } = HelpMessageKey;
    public bool ShowInHelp { get; } = ShowInHelp;
    public int DisplayOrder { get; } = DisplayOrder;
}
