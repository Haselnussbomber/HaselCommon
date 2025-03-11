using System.Globalization;
using Lumina.Text.ReadOnly;

using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;
using LSeString = Lumina.Text.SeString;

namespace HaselCommon.Services.Evaluator;

/// <summary>
/// A wrapper for a local parameter, holding either a number or a string.
/// </summary>
public readonly struct SeStringParameter
{
    private readonly uint _num;
    private readonly ReadOnlySeString _str;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeStringParameter"/> struct for a number parameter.
    /// </summary>
    /// <param name="value">The number value.</param>
    public SeStringParameter(uint value)
    {
        _num = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SeStringParameter"/> struct for a string parameter.
    /// </summary>
    /// <param name="value">The string value.</param>
    public SeStringParameter(ReadOnlySeString value)
    {
        _str = value;
        IsString = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SeStringParameter"/> struct for a string parameter.
    /// </summary>
    /// <param name="value">The string value.</param>
    public SeStringParameter(string value)
    {
        _str = new ReadOnlySeString(value);
        IsString = true;
    }

    /// <summary>
    /// Gets a value indicating whether the backing type of this parameter is a string.
    /// </summary>
    public bool IsString { get; }

    /// <summary>
    /// Gets a numeric value.
    /// </summary>
    public uint UIntValue =>
        !IsString
            ? _num
            : uint.TryParse(_str.ExtractText(), out var value) ? value : 0;

    /// <summary>
    /// Gets a string value.
    /// </summary>
    public ReadOnlySeString StringValue =>
        IsString ? _str : new(_num.ToString("D", CultureInfo.InvariantCulture));

    public static implicit operator SeStringParameter(int value) => new((uint)value);

    public static implicit operator SeStringParameter(uint value) => new(value);

    public static implicit operator SeStringParameter(ReadOnlySeString value) => new(value);

    public static implicit operator SeStringParameter(ReadOnlySeStringSpan value) => new(new ReadOnlySeString(value));

    public static implicit operator SeStringParameter(LSeString value) => new(new ReadOnlySeString(value.RawData));

    public static implicit operator SeStringParameter(DSeString value) => new(new ReadOnlySeString(value.Encode()));

    public static implicit operator SeStringParameter(string value) => new(value);
}
