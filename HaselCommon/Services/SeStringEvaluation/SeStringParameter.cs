using Lumina.Text.ReadOnly;

using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;
using LSeString = Lumina.Text.SeString;

namespace HaselCommon.Services.SeStringEvaluation;

public readonly struct SeStringParameter
{
    private readonly uint _num;
    private readonly ReadOnlySeString _str;

    public bool IsString { get; }

    public uint UIntValue
    {
        get
        {
            if (!IsString)
                return uint.TryParse(_str.ExtractText(), out var value) ? value : 0;

            return _num;
        }
    }

    public ReadOnlySeString StringValue
    {
        get
        {
            if (IsString)
                return new ReadOnlySeString(_num.ToString());

            return _str;
        }
    }

    public SeStringParameter(uint value)
    {
        IsString = true;
        _num = value;
    }

    public SeStringParameter(ReadOnlySeString value)
    {
        _str = value;
    }

    public SeStringParameter(string value)
    {
        _str = new ReadOnlySeString(value);
    }

    public static implicit operator SeStringParameter(int value) => new((uint)value);
    public static implicit operator SeStringParameter(uint value) => new(value);
    public static implicit operator SeStringParameter(ReadOnlySeString value) => new(value);
    public static implicit operator SeStringParameter(ReadOnlySeStringSpan value) => new(new ReadOnlySeString(value));
    public static implicit operator SeStringParameter(LSeString value) => new(new ReadOnlySeString(value.RawData));
    public static implicit operator SeStringParameter(DSeString value) => new(new ReadOnlySeString(value.Encode()));
    public static implicit operator SeStringParameter(string value) => new(value);
}
