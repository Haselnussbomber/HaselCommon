namespace HaselCommon.Commands.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandHandlerAttribute(string Command, string HelpMessageKey) : Attribute
{
    public string Command { get; } = Command;
    public string HelpMessageKey { get; } = HelpMessageKey;
}
