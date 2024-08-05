using System.Collections.Generic;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using HaselCommon.Commands;
using HaselCommon.Extensions;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

public class CommandService(
    ILogger<CommandService> Logger,
    ICommandManager CommandManager,
    TranslationManager TranslationManager) : IDisposable
{
    private readonly Dictionary<string, CommandHandler> CommandHandlers = [];

    public void Dispose()
    {
        CommandHandlers.Dispose();
        GC.SuppressFinalize(this);
    }

    public CommandHandler? Register(IReadOnlyCommandInfo.HandlerDelegate handler, bool enabled = true)
    {
        var attr = handler.Method.GetCustomAttribute<CommandHandlerAttribute>()
            ?? throw new Exception($"Missing CommandHandlerAttribute on {handler.Method.Name}");

        return new CommandHandler(
            this,
            CommandManager,
            TranslationManager,
            attr.Command,
            attr.HelpMessageKey,
            attr.ShowInHelp,
            handler,
            enabled);
    }

    public CommandHandler Register(CommandHandler commandHandler)
    {
        Logger.LogDebug("Registering {command}", commandHandler.Command);
        CommandHandlers.Add(commandHandler.Command, commandHandler);
        return commandHandler;
    }

    public void Unregister(CommandHandler commandHandler)
    {
        if (CommandHandlers.ContainsKey(commandHandler.Command))
        {
            Logger.LogDebug("Unregistering {command}", commandHandler.Command);
            CommandHandlers.Remove(commandHandler.Command);
            commandHandler.Dispose();
        }
    }
}
