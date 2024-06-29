using Dalamud.Game.Command;

namespace HaselCommon.Commands.Interfaces;

public interface ICommandHandler : IDisposable
{
    string Command { get; }
    string HelpMessageKey { get; }
    IReadOnlyCommandInfo.HandlerDelegate Handler { get; }
    bool IsEnabled { get; }

    void SetEnabled(bool enabled);
    void Unregister();
}
