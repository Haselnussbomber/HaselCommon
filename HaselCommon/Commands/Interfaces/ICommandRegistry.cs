using Dalamud.Game.Command;

namespace HaselCommon.Commands.Interfaces;

public interface ICommandRegistry
{
    ICommandHandler? Register(string command, string helpMessageKey, CommandInfo.HandlerDelegate handler);
    ICommandHandler? Register(CommandInfo.HandlerDelegate handler);
    void Unregister(string command);
    void Unregister(ICommandHandler commandHandler);
    void Unregister(CommandInfo.HandlerDelegate handler);
}
