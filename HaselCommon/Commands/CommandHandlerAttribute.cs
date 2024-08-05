namespace HaselCommon.Commands;

[AttributeUsage(AttributeTargets.Method)]
public class CommandHandlerAttribute(string Command, string HelpMessageKey, bool ShowInHelp = true) : Attribute
{
    public string Command { get; } = Command;
    public string HelpMessageKey { get; } = HelpMessageKey;
    public bool ShowInHelp { get; } = ShowInHelp;
}
