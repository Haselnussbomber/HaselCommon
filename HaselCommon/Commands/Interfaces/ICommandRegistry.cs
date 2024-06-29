using Dalamud.Game.Command;

namespace HaselCommon.Commands.Interfaces;

public interface ICommandRegistry
{
    ICommandHandler? Register(string command, string helpMessageKey, IReadOnlyCommandInfo.HandlerDelegate handler);
    ICommandHandler? Register(IReadOnlyCommandInfo.HandlerDelegate handler);
    void Unregister(string command);
    void Unregister(ICommandHandler commandHandler);
    void Unregister(IReadOnlyCommandInfo.HandlerDelegate handler);
}
