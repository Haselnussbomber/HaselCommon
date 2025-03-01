using System.Collections.Generic;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using HaselCommon.Commands;
using HaselCommon.Extensions.Collections;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

[RegisterSingleton]
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
            attr.DisplayOrder,
            handler,
            autoEnable);
    }

    public CommandHandler Register(string command, string helpMessageKey, IReadOnlyCommandInfo.HandlerDelegate handler, bool showInHelp = true, int displayOrder = -1, bool autoEnable = false)
    {
        return new CommandHandler(
            this,
            commandManager,
            languageProvider,
            textService,
            command,
            helpMessageKey,
            showInHelp,
            displayOrder,
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
