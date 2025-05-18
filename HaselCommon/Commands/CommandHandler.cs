using Dalamud.Game.Command;

namespace HaselCommon.Commands;

public class CommandHandler : IDisposable
{
    private readonly CommandService _commandService;
    private readonly ICommandManager _dalamudCommandManager;
    private readonly LanguageProvider _languageProvider;
    private readonly TextService _textService;
    private CommandInfo? _commandInfo;
    private bool _isDisposed;

    public string Command { get; init; }
    public string HelpMessageKey { get; init; }
    public bool ShowInHelp { get; init; }
    public int DisplayOrder { get; init; } = -1;
    public IReadOnlyCommandInfo.HandlerDelegate Handler { get; init; }

    public bool IsEnabled { get; private set; }

    public CommandHandler(
        CommandService commandService,
        ICommandManager dalamudCommandManager,
        LanguageProvider languageProvider,
        TextService textService,
        string command,
        string helpMessageKey,
        bool showInHelp,
        int displayOrder,
        IReadOnlyCommandInfo.HandlerDelegate handler,
        bool enabled = true)
    {
        _commandService = commandService;
        _dalamudCommandManager = dalamudCommandManager;
        _languageProvider = languageProvider;
        _textService = textService;

        Command = command;
        HelpMessageKey = helpMessageKey;
        ShowInHelp = showInHelp;
        DisplayOrder = displayOrder;
        Handler = handler;

        _languageProvider.LanguageChanged += OnLanguageChanged;

        _commandService.Register(this);

        SetEnabled(enabled);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _languageProvider.LanguageChanged -= OnLanguageChanged;

        SetEnabled(false);
        _commandService.Unregister(this);
        _isDisposed = true;
    }

    private void OnLanguageChanged(string langCode)
    {
        if (_commandInfo == null || string.IsNullOrEmpty(HelpMessageKey))
            return;

        _commandInfo.HelpMessage = _textService.Translate(HelpMessageKey);
    }

    public void SetEnabled(bool enabled)
    {
        if (_isDisposed) return;

        if (!IsEnabled && enabled)
        {
            _dalamudCommandManager.AddHandler(Command, _commandInfo = new CommandInfo(Handler)
            {
                HelpMessage = !string.IsNullOrEmpty(HelpMessageKey) ? _textService.Translate(HelpMessageKey) : string.Empty,
                ShowInHelp = ShowInHelp,
                DisplayOrder = DisplayOrder,
            });

            IsEnabled = true;
        }
        else if (IsEnabled && !enabled)
        {
            _dalamudCommandManager.RemoveHandler(Command);
            _commandInfo = null;
            IsEnabled = false;
        }
    }
}
