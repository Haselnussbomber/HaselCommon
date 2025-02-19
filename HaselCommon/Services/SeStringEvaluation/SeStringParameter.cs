using Lumina.Text.ReadOnly;

using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;
using LSeString = Lumina.Text.SeString;

namespace HaselCommon.Services.SeStringEvaluation;

public readonly struct SeStringParameter
{
    private readonly uint _num;
    private readonly ReadOnlySeString _str;

    public bool IsString { get; }
    public uint UIntValue => IsString ? (uint.TryParse(_str.ExtractText(), out var value) ? value : 0) : _num;
    public ReadOnlySeString StringValue => IsString ? _str : new ReadOnlySeString(_num.ToString());

    public SeStringParameter(uint value)
    {
        _num = value;
    }

    public SeStringParameter(ReadOnlySeString value)
    {
        _str = value;
        IsString = true;
    }

    public SeStringParameter(string value)
    {
        _str = new ReadOnlySeString(value);
        IsString = true;
    }

    public static implicit operator SeStringParameter(int value) => new((uint)value);
    public static implicit operator SeStringParameter(uint value) => new(value);
    public static implicit operator SeStringParameter(ReadOnlySeString value) => new(value);
    public static implicit operator SeStringParameter(ReadOnlySeStringSpan value) => new(new ReadOnlySeString(value));
    public static implicit operator SeStringParameter(LSeString value) => new(new ReadOnlySeString(value.RawData));
    public static implicit operator SeStringParameter(DSeString value) => new(new ReadOnlySeString(value.Encode()));
    public static implicit operator SeStringParameter(string value) => new(value);
}
