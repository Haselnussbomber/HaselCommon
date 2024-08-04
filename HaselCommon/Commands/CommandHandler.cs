using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using HaselCommon.Commands.Interfaces;
using HaselCommon.Services;

namespace HaselCommon.Commands;

public class CommandHandler : ICommandHandler
{
    private readonly CommandRegistry CommandManager;
    private readonly ICommandManager DalamudCommandManager;
    private readonly TranslationManager TranslationManager;
    private CommandInfo? CommandInfo;

    public string Command { get; init; }
    public string HelpMessageKey { get; init; }
    public bool ShowInHelp { get; init; }
    public IReadOnlyCommandInfo.HandlerDelegate Handler { get; init; }
    public bool IsEnabled { get; private set; }

    public CommandHandler(
        CommandRegistry commandManager,
        ICommandManager dalamudCommandManager,
        TranslationManager translationManager,
        string command,
        string helpMessageKey,
        bool showInHelp,
        IReadOnlyCommandInfo.HandlerDelegate handler)
    {
        CommandManager = commandManager;
        DalamudCommandManager = dalamudCommandManager;
        TranslationManager = translationManager;
        Command = command;
        HelpMessageKey = helpMessageKey;
        ShowInHelp = showInHelp;
        Handler = handler;

        TranslationManager.LanguageChanged += TranslationManager_LanguageChanged;
    }

    public void Dispose()
    {
        SetEnabled(false);
        TranslationManager.LanguageChanged -= TranslationManager_LanguageChanged;
        GC.SuppressFinalize(this);
    }

    private void TranslationManager_LanguageChanged(string langCode)
    {
        if (CommandInfo == null || string.IsNullOrEmpty(HelpMessageKey))
            return;

        CommandInfo.HelpMessage = TranslationManager.Translate(HelpMessageKey);
    }

    public void SetEnabled(bool enabled)
    {
        if (!IsEnabled && enabled)
        {
            DalamudCommandManager.AddHandler(Command, CommandInfo = new CommandInfo(Handler)
            {
                HelpMessage = !string.IsNullOrEmpty(HelpMessageKey) ? TranslationManager.Translate(HelpMessageKey) : string.Empty,
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

    public void Unregister()
    {
        CommandManager.Unregister(this);
    }
}
