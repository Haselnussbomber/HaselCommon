using System.Collections.Generic;
using System.Reflection;
using Dalamud.Plugin.Services;
using HaselCommon.Attributes;
using HaselCommon.Extensions;
using Microsoft.Extensions.Logging;
using static Dalamud.Game.Command.CommandInfo;

namespace HaselCommon.Services.CommandManager;

public sealed class CommandManager(
    ICommandManager DalamudCommandManager,
    TranslationManager TranslationManager,
    ILogger<CommandManager> Logger)
    : IDisposable
{
    private readonly Dictionary<string, CommandHandler> CommandHandlers = [];

    public void Dispose()
    {
        CommandHandlers.Dispose();
    }

    public CommandHandler? Register(HandlerDelegate handler)
    {
        var attr = handler.Method.GetCustomAttribute<CommandHandlerAttribute>()
            ?? throw new Exception($"Missing CommandHandlerAttribute on {handler.Method.Name}");

        var command = attr.Command;
        var helpMessageKey = attr.HelpMessageKey;

        Logger.LogDebug("Registring {command}", command);

        var commandHandler = new CommandHandler(this, DalamudCommandManager, TranslationManager, command, helpMessageKey, handler);
        CommandHandlers.Add(command, commandHandler);
        return commandHandler;
    }

    public void Unregister(string command)
    {
        if (CommandHandlers.TryGetValue(command, out var commandHandler))
        {
            commandHandler.Dispose();
            CommandHandlers.Remove(command);
        }
    }

    public void Unregister(CommandHandler commandHandler)
    {
        Unregister(commandHandler.Command);
    }

    public void Unregister(HandlerDelegate handler)
    {
        var attr = handler.Method.GetCustomAttribute<CommandHandlerAttribute>()
            ?? throw new Exception($"Missing CommandHandlerAttribute on {handler.Method.Name}");

        Unregister(attr.Command);
    }
}
