using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services.SeStringEvaluation;

public struct SeStringParameter
{
    private uint? num = null;
    private ReadOnlySeString? str = null;

    public bool IsString { get; private set; }

    public uint UIntValue
    {
        get
        {
            if (!IsString)
                return num ?? 0;

            var text = (str ?? new()).ExtractText();
            if (string.IsNullOrWhiteSpace(text) || !uint.TryParse(text, out var value))
                return 0;

            return value;
        }
        set
        {
            num = value;
            str = null;
            IsString = false;
        }
    }

    public ReadOnlySeString StringValue
    {
        get
        {
            if (!IsString && num != null)
                return new SeStringBuilder().Append((num ?? 0).ToString()).ToReadOnlySeString();

            return str ?? new();
        }
        set
        {
            num = null;
            str = value;
            IsString = true;
        }
    }

    public SeStringParameter(uint value)
    {
        UIntValue = value;
    }

    public SeStringParameter(ReadOnlySeString value)
    {
        StringValue = value;
    }

    public SeStringParameter(string value)
    {
        StringValue = new SeStringBuilder().Append(value).ToReadOnlySeString();
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
