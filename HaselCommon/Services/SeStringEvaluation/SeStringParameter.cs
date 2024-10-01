using Lumina.Text.ReadOnly;

using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;
using LSeString = Lumina.Text.SeString;

namespace HaselCommon.Services.SeStringEvaluation;

public struct SeStringParameter
{
    private uint? _num = null;
    private ReadOnlySeString? _str = null;

    public bool IsString { get; private set; }

    public uint UIntValue
    {
        get
        {
            if (!IsString)
                return _num ?? 0;

            var text = (_str ?? new()).ExtractText();
            if (string.IsNullOrWhiteSpace(text) || !uint.TryParse(text, out var value))
                return 0;

            return value;
        }
        set
        {
            _num = value;
            _str = null;
            IsString = false;
        }
    }

    public ReadOnlySeString StringValue
    {
        get
        {
            if (!IsString && _num != null)
                return ReadOnlySeString.FromText((_num ?? 0).ToString());

            return _str ?? new();
        }
        set
        {
            _num = null;
            _str = value;
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
        StringValue = ReadOnlySeString.FromText(value);
    }

    public static implicit operator SeStringParameter(int value)
        => new((uint)value);

    public static implicit operator SeStringParameter(uint value)
        => new(value);

    public static implicit operator SeStringParameter(ReadOnlySeString value)
        => new(value);

    public static implicit operator SeStringParameter(ReadOnlySeStringSpan value)
        => new(new ReadOnlySeString(value));

    public static implicit operator SeStringParameter(LSeString value)
        => new(new ReadOnlySeString(value));

    public static implicit operator SeStringParameter(DSeString value)
        => new(new ReadOnlySeString(value.Encode()));

    public static implicit operator SeStringParameter(string value)
        => new(value);
}
