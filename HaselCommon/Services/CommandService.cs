using System.Text;
using Dalamud.Game.Command;
using Dalamud.Utility;
using HaselCommon.Services.Commands;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class CommandService : IDisposable
{
    private readonly ILogger<CommandService> _logger;
    private readonly ICommandManager _commandManager;
    private readonly TextService _textService;
    private readonly LanguageProvider _languageProvider;
    private readonly Dictionary<string, CommandHandler> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CommandInfo> _commandInfos = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _commandState = new(StringComparer.OrdinalIgnoreCase);

    [AutoPostConstruct]
    private void Initialize()
    {
        _languageProvider.LanguageChanged += OnLanguageChanged;
    }

    public void Dispose()
    {
        _languageProvider.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(string lang)
    {
        foreach (var command in _commands.Values)
            UpdateRecursive(command);

        static void UpdateRecursive(CommandHandler command)
        {
            command.UpdateHelpText();

            foreach (var child in command.Children.Values)
                UpdateRecursive(child);
        }
    }

    public CommandHandler AddCommand(string name, Action<CommandHandler>? callback = null)
    {
        var commandInfo = new CommandInfo(DalamudHandler);
        var node = new CommandHandler(name, this)
        {
            CommandInfo = commandInfo
        };

        _commandInfos[name] = commandInfo;
        _commands[name] = node;

        callback?.Invoke(node);

        if (node.Enabled)
            EnableCommand(node);

        return node;
    }

    internal void RemoveCommand(CommandHandler node)
    {
        DisableCommand(node);
        _commands.Remove(node.Name);
        _commandInfos.Remove(node.Name);
    }

    internal void EnableCommand(CommandHandler node)
    {
        if (_commandState.TryGetValue(node.Name, out var enabled) && enabled)
            return;

        _logger.LogInformation("Enabling command /{name}", node.Name);

        if (_commandInfos.TryGetValue(node.Name, out var handler))
        {
            _commandManager.AddHandler($"/{node.Name}", handler);
            _commandState[node.Name] = true;
        }
    }

    internal void DisableCommand(CommandHandler node)
    {
        if (_commandState.TryGetValue(node.Name, out var enabled) && !enabled)
            return;

        _logger.LogInformation("Disabling command /{name}", node.Name);

        _commandManager.RemoveHandler($"/{node.Name}");
        _commandState[node.Name] = false;
    }

    private void DalamudHandler(string command, string arguments)
    {
        var cmdName = command.StartsWith('/') ? command[1..] : command;

        if (!_commands.TryGetValue(cmdName, out var current))
            return;

        var args = new List<string>();

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            var sb = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < arguments.Length; i++)
            {
                var c = arguments[i];

                if (c == '\\' && i + 1 < arguments.Length && arguments[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        args.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }

                sb.Append(c);
            }

            if (sb.Length > 0)
                args.Add(sb.ToString());
        }

        if (args.Count > 0 && args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            ShowHelp(current);
            return;
        }

        var index = 0;
        while (index < args.Count && current.Children.TryGetValue(args[index], out var next) && next.Enabled)
        {
            current = next;
            index++;
        }

        if (current.Handler == null || (args.Count > 0 && args[^1].Equals("help", StringComparison.OrdinalIgnoreCase)))
        {
            ShowHelp(current);
            return;
        }

        var parsedArgs = ParseArgs(current, [.. args.Skip(index)]);
        if (parsedArgs == null)
        {
            ShowHelp(current);
            return;
        }

        current.Handler(new CommandContext(parsedArgs));
    }

    private void ShowHelp(CommandHandler node)
    {
        using var rssb = new RentedSeStringBuilder();
        rssb.Builder.Append(node.GetFullPath());

        if (!string.IsNullOrEmpty(node.HelpTextKey))
        {
            rssb.Builder
                .AppendNewLine()
                .Append("→ ")
                .Append(_textService.Translate(node.HelpTextKey));
        }

        if (node.Args.Count != 0)
        {
            var argDisplay = string.Join(" ", node.Args.Select(a => $"<{a.Name}>"));
            rssb.Builder
                .AppendNewLine()
                .Append(_textService.EvaluateTranslatedSeString("CommandManager.Usage"))
                .Append($"{node.GetFullPath()} {argDisplay}");
        }

        if (node.Children.Any(kv => kv.Value.Enabled))
        {
            rssb.Builder
                .AppendNewLine()
                .Append(_textService.EvaluateTranslatedSeString("CommandManager.Subcommands"));

            foreach (var child in node.Children.Values)
            {
                if (!child.Enabled)
                    continue;

                var desc = child.HelpTextKey != null
                    ? _textService.Translate(child.HelpTextKey)
                    : _textService.EvaluateTranslatedSeString("CommandManager.NoDescription");

                rssb.Builder
                    .AppendNewLine()
                    .Append("  ")
                    .PushColorType(506)
                    .PushEdgeColorType(507)
                    .Append(child.Name)
                    .PopEdgeColorType()
                    .PopColorType()
                    .Append($"  {desc}");
            }
        }

        Chat.Print(rssb.Builder.GetViewAsSpan());
    }

    private static Dictionary<string, string>? ParseArgs(CommandHandler node, List<string> tokens)
    {
        var result = new Dictionary<string, string>();

        for (var i = 0; i < node.Args.Count; i++)
        {
            var arg = node.Args[i];

            if (i >= tokens.Count)
                return arg.Required ? null : result;

            result[arg.Name] = arg.ConsumeRest ? string.Join(" ", tokens.Skip(i)) : tokens[i];

            if (arg.ConsumeRest)
                break;
        }

        return result;
    }
}
