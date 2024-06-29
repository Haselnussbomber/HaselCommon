using System.Collections.Generic;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using HaselCommon.Commands.Attributes;
using HaselCommon.Commands.Interfaces;
using HaselCommon.Extensions;
using HaselCommon.Services;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Commands;

public class CommandRegistry(
    ILogger<CommandRegistry> Logger,
    ICommandManager DalamudCommandManager,
    TranslationManager TranslationManager)
    : ICommandRegistry, IDisposable
{
    private readonly Dictionary<string, ICommandHandler> CommandHandlers = [];

    public void Dispose()
    {
        CommandHandlers.Dispose();
        GC.SuppressFinalize(this);
    }

    public ICommandHandler? Register(string command, string helpMessageKey, IReadOnlyCommandInfo.HandlerDelegate handler)
    {
        Logger.LogDebug("Registring {command}", command);

        var commandHandler = new CommandHandler(this, DalamudCommandManager, TranslationManager, command, helpMessageKey, handler);
        CommandHandlers.Add(command, commandHandler);
        return commandHandler;
    }

    public ICommandHandler? Register(IReadOnlyCommandInfo.HandlerDelegate handler)
    {
        var attr = handler.Method.GetCustomAttribute<CommandHandlerAttribute>()
            ?? throw new Exception($"Missing CommandHandlerAttribute on {handler.Method.Name}");

        return Register(attr.Command, attr.HelpMessageKey, handler);
    }

    public void Unregister(string command)
    {
        if (CommandHandlers.TryGetValue(command, out var commandHandler))
        {
            commandHandler.Dispose();
            CommandHandlers.Remove(command);
        }
    }

    public void Unregister(ICommandHandler commandHandler)
    {
        Unregister(commandHandler.Command);
    }

    public void Unregister(IReadOnlyCommandInfo.HandlerDelegate handler)
    {
        var attr = handler.Method.GetCustomAttribute<CommandHandlerAttribute>()
            ?? throw new Exception($"Missing CommandHandlerAttribute on {handler.Method.Name}");

        Unregister(attr.Command);
    }
}
