using Dalamud.Game.Command;

namespace HaselCommon.Services.Commands;

public class CommandHandler : IDisposable
{
    private readonly CommandService _commandService;

    public string Name { get; init; }

    public bool Enabled
    {
        get;
        set
        {
            if (field = value)
                _commandService?.EnableCommand(this);
            else
                _commandService?.DisableCommand(this);
        }
    } = true;

    public bool ShowInHelp
    {
        get;
        set
        {
            field = value;
            CommandInfo?.ShowInHelp = value;
        }
    } = true;

    public int DisplayOrder
    {
        get;
        set
        {
            field = value;
            CommandInfo?.DisplayOrder = value;
        }
    } = -1;

    public string? HelpTextKey
    {
        get;
        set
        {
            field = value;
            UpdateHelpText();
        }
    } = null;

    public Action<CommandContext>? Handler { get; set; }

    internal List<CommandArg> Args { get; } = [];
    internal Dictionary<string, CommandHandler> Children { get; } = new(StringComparer.OrdinalIgnoreCase);
    internal CommandHandler? Parent { get; init; }
    internal CommandInfo? CommandInfo { get; set; }

    internal CommandHandler(string name, CommandService commandManager)
    {
        Name = name;
        _commandService = commandManager;
    }

    public void Dispose()
    {
        if (Parent != null)
            Parent.RemoveSubcommand(this);
        else
            _commandService?.RemoveCommand(this);
    }

    public string GetFullPath()
    {
        return Parent == null
            ? $"/{Name}"
            : $"{Parent.GetFullPath()} {Name}";
    }

    public CommandHandler AddSubcommand(string name, Action<CommandHandler>? callback = null)
    {
        var node = new CommandHandler(name, _commandService)
        {
            Parent = this
        };
        Children[node.Name] = node;
        callback?.Invoke(node);
        return this;
    }

    public void RemoveSubcommand(string name)
    {
        Children.Remove(name);
    }

    public void RemoveSubcommand(CommandHandler commandNode)
    {
        Children.Remove(commandNode.Name);
    }

    public CommandHandler SetEnabled(bool enabled)
    {
        Enabled = enabled;
        return this;
    }

    public CommandHandler WithHelpTextKey(string key)
    {
        HelpTextKey = key;
        return this;
    }

    public CommandHandler WithDisplayOrder(int order)
    {
        DisplayOrder = order;
        return this;
    }

    public CommandHandler WithHandler(Action<CommandContext> handler)
    {
        Handler = handler;
        return this;
    }

    public CommandHandler WithArg<T>(string name, bool required = true, bool consumeRest = false)
    {
        Args.Add(new CommandArg(name, typeof(Type), required, consumeRest));
        return this;
    }

    internal void UpdateHelpText()
    {
        if (CommandInfo == null)
            return;

        CommandInfo.HelpMessage = !string.IsNullOrEmpty(HelpTextKey)
            ? _commandService._textService.Translate(HelpTextKey)
            : string.Empty;
    }
}
