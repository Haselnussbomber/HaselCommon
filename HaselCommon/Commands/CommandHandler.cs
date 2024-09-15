using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using HaselCommon.Services;

namespace HaselCommon.Commands;

public class CommandHandler : IDisposable
{
    private readonly CommandService CommandService;
    private readonly ICommandManager DalamudCommandManager;
    private readonly TextService TextService;
    private CommandInfo? CommandInfo;
    private bool IsDisposed;

    public string Command { get; init; }
    public string HelpMessageKey { get; init; }
    public bool ShowInHelp { get; init; }
    public IReadOnlyCommandInfo.HandlerDelegate Handler { get; init; }

    public bool IsEnabled { get; private set; }

    public CommandHandler(
        CommandService commandService,
        ICommandManager dalamudCommandManager,
        TextService textService,
        string command,
        string helpMessageKey,
        bool showInHelp,
        IReadOnlyCommandInfo.HandlerDelegate handler,
        bool enabled = true)
    {
        CommandService = commandService;
        DalamudCommandManager = dalamudCommandManager;
        TextService = textService;

        Command = command;
        HelpMessageKey = helpMessageKey;
        ShowInHelp = showInHelp;
        Handler = handler;

        TextService.LanguageChanged += OnLanguageChanged;

        CommandService.Register(this);

        SetEnabled(enabled);
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        TextService.LanguageChanged -= OnLanguageChanged;

        SetEnabled(false);
        CommandService.Unregister(this);
        IsDisposed = true;

        GC.SuppressFinalize(this);
    }

    private void OnLanguageChanged(string langCode)
    {
        if (CommandInfo == null || string.IsNullOrEmpty(HelpMessageKey))
            return;

        CommandInfo.HelpMessage = TextService.Translate(HelpMessageKey);
    }

    public void SetEnabled(bool enabled)
    {
        if (IsDisposed) return;

        if (!IsEnabled && enabled)
        {
            DalamudCommandManager.AddHandler(Command, CommandInfo = new CommandInfo(Handler)
            {
                HelpMessage = !string.IsNullOrEmpty(HelpMessageKey) ? TextService.Translate(HelpMessageKey) : string.Empty,
                ShowInHelp = ShowInHelp,
            });

            IsEnabled = true;
        }
        else if (IsEnabled && !enabled)
        {
            DalamudCommandManager.RemoveHandler(Command);
            CommandInfo = null;
            IsEnabled = false;
        }
    }
}
