using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using static Dalamud.Game.Command.CommandInfo;

namespace HaselCommon.Services.CommandManager;

public class CommandHandler : IDisposable
{
    private readonly CommandManager CommandManager;
    private readonly ICommandManager DalamudCommandManager;
    private readonly TranslationManager TranslationManager;
    private CommandInfo? CommandInfo;

    public string Command { get; init; }
    public string HelpMessageKey { get; init; }
    public HandlerDelegate Handler { get; init; }
    public bool IsEnabled { get; private set; }

    public CommandHandler(
        CommandManager commandManager,
        ICommandManager dalamudCommandManager,
        TranslationManager translationManager,
        string command,
        string helpMessageKey,
        HandlerDelegate handler)
    {
        CommandManager = commandManager;
        DalamudCommandManager = dalamudCommandManager;
        TranslationManager = translationManager;
        Command = command;
        HelpMessageKey = helpMessageKey;
        Handler = handler;

        TranslationManager.LanguageChanged += TranslationManager_LanguageChanged;
    }

    public void Dispose()
    {
        SetEnabled(false);
        TranslationManager.LanguageChanged -= TranslationManager_LanguageChanged;
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
                HelpMessage = !string.IsNullOrEmpty(HelpMessageKey) ? TranslationManager.Translate(HelpMessageKey) : string.Empty
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
