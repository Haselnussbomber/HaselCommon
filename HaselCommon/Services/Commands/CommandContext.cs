namespace HaselCommon.Services.Commands;

public record struct CommandContext(Dictionary<string, string> Args)
{
    public bool TryGet(string name, out string value)
    {
        if (Args.TryGetValue(name, out var arg))
        {
            value = arg;
            return true;
        }
        value = string.Empty;
        return false;
    }

    public bool TryGet(string name, out bool value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && bool.TryParse(arg, out value);
    }

    public bool TryGet(string name, out sbyte value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && sbyte.TryParse(arg, out value);
    }

    public bool TryGet(string name, out byte value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && byte.TryParse(arg, out value);
    }

    public bool TryGet(string name, out short value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && short.TryParse(arg, out value);
    }

    public bool TryGet(string name, out ushort value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && ushort.TryParse(arg, out value);
    }

    public bool TryGet(string name, out int value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && int.TryParse(arg, out value);
    }

    public bool TryGet(string name, out uint value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && uint.TryParse(arg, out value);
    }

    public bool TryGet(string name, out long value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && long.TryParse(arg, out value);
    }

    public bool TryGet(string name, out ulong value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && ulong.TryParse(arg, out value);
    }

    public bool TryGet(string name, out float value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && float.TryParse(arg, out value);
    }

    public bool TryGet(string name, out double value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && double.TryParse(arg, out value);
    }

    public bool TryGet(string name, out decimal value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && decimal.TryParse(arg, out value);
    }

    public bool TryGet(string name, out DateTime value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && DateTime.TryParse(arg, out value);
    }

    public bool TryGet(string name, out DateTimeOffset value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && DateTimeOffset.TryParse(arg, out value);
    }

    public bool TryGet(string name, out TimeSpan value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && TimeSpan.TryParse(arg, out value);
    }

    public bool TryGet(string name, out Guid value)
    {
        value = default;
        return Args.TryGetValue(name, out var arg) && Guid.TryParse(arg, out value);
    }
}
