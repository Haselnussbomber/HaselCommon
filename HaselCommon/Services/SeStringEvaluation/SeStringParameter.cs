using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services.SeStringEvaluation;

public struct SeStringParameter
{
    private readonly uint? val = null;
    private bool isString;
    private ReadOnlySeString? str = null;

    public uint AsUInt()
    {
        if (!isString)
            return val ?? 0;

        var text = (str ?? new()).ExtractText();
        if (string.IsNullOrWhiteSpace(text) || !uint.TryParse(text, out var value))
            return 0;

        return value;
    }

    public ReadOnlySeString AsString()
    {
        if (!isString && str == null)
        {
            str = new SeStringBuilder().Append(val.ToString()).ToReadOnlySeString();
            isString = true;
        }

        return str ?? new();
    }

    public SeStringParameter(uint value)
    {
        isString = false;
        val = value;
    }

    public SeStringParameter(ReadOnlySeString value)
    {
        isString = true;
        str = value;
    }

    public SeStringParameter(string value)
    {
        isString = true;
        str = new SeStringBuilder().Append(value).ToReadOnlySeString();
    }

    public static implicit operator SeStringParameter(int value)
        => new((uint)value);

    public static implicit operator SeStringParameter(uint value)
        => new(value);

    public static implicit operator SeStringParameter(ReadOnlySeString value)
        => new(value);

    public static implicit operator SeStringParameter(ReadOnlySeStringSpan value)
        => new(new ReadOnlySeString(new ReadOnlyMemory<byte>(value.Data.ToArray())));

    public static implicit operator SeStringParameter(SeString value)
        => new(new ReadOnlySeString(new ReadOnlyMemory<byte>(value.RawData.ToArray())));

    public static implicit operator SeStringParameter(string value)
        => new(value);
}
