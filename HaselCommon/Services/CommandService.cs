using System.Reflection;
using Dalamud.Game.Command;
using HaselCommon.Commands;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class CommandService : IDisposable
{
    private readonly ILogger<CommandService> _logger;
    private readonly ICommandManager _commandManager;
    private readonly LanguageProvider _languageProvider;
    private readonly TextService _textService;

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
            _commandManager,
            _languageProvider,
            _textService,
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
            _commandManager,
            _languageProvider,
            _textService,
            command,
            helpMessageKey,
            showInHelp,
            displayOrder,
            handler,
            autoEnable);
    }

    public CommandHandler Register(CommandHandler commandHandler)
    {
        _logger.LogDebug("Registering {command}", commandHandler.Command);
        _commandHandlers.Add(commandHandler.Command, commandHandler);
        return commandHandler;
    }

    public void Unregister(CommandHandler commandHandler)
    {
        if (_commandHandlers.ContainsKey(commandHandler.Command))
        {
            _logger.LogDebug("Unregistering {command}", commandHandler.Command);
            _commandHandlers.Remove(commandHandler.Command);
            commandHandler.Dispose();
        }
    }
}
