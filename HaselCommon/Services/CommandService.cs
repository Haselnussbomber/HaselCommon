using System.Collections.Generic;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using HaselCommon.Commands;
using HaselCommon.Extensions.Collections;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

public class CommandService(
    ILogger<CommandService> logger,
    ICommandManager commandManager,
    LanguageProvider languageProvider,
    TextService textService) : IDisposable
{
    private readonly Dictionary<string, CommandHandler> _commandHandlers = [];

    public void Dispose()
    {
        _commandHandlers.Dispose();
        GC.SuppressFinalize(this);
    }

    public CommandHandler? Register(IReadOnlyCommandInfo.HandlerDelegate handler, bool autoEnable = false)
    {
        var attr = handler.Method.GetCustomAttribute<CommandHandlerAttribute>()
            ?? throw new Exception($"Missing CommandHandlerAttribute on {handler.Method.Name}");

        return new CommandHandler(
            this,
            commandManager,
            languageProvider,
            textService,
            attr.Command,
            attr.HelpMessageKey,
            attr.ShowInHelp,
            handler,
            autoEnable);
    }

    public CommandHandler Register(CommandHandler commandHandler)
    {
        logger.LogDebug("Registering {command}", commandHandler.Command);
        _commandHandlers.Add(commandHandler.Command, commandHandler);
        return commandHandler;
    }

    public void Unregister(CommandHandler commandHandler)
    {
        if (_commandHandlers.ContainsKey(commandHandler.Command))
        {
            logger.LogDebug("Unregistering {command}", commandHandler.Command);
            _commandHandlers.Remove(commandHandler.Command);
            commandHandler.Dispose();
        }
    }
}
