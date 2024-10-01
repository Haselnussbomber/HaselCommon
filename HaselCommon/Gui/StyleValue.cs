using System.Globalization;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui;

#pragma warning disable CS0660
#pragma warning disable CS0661

public struct StyleValue
{
    public float Value;
    public Unit Unit;

    public static StyleValue Undefined
        => new() { Value = 0, Unit = Unit.Undefined };

    public static StyleValue Auto
        => new() { Value = float.NaN, Unit = Unit.Auto };

    public static StyleValue Percent(float value)
        => new() { Value = value, Unit = Unit.Percent };

    public static StyleValue Point(float value)
        => new() { Value = value, Unit = Unit.Point };

    public static implicit operator StyleValue(float value) => Point(value);

    public bool IsDefined => !float.IsNaN(Value) && Value != 0;
    public bool IsAuto => Unit is Unit.Auto;

    public float Resolve(float referenceLength)
    {
        return Unit switch
        {
            Unit.Point => Value,
            Unit.Percent => Value * referenceLength * 0.01f,
            _ => float.NaN
        };
    }

    public float ResolveOrDefault(float referenceLength, float defaultValue)
    {
        var value = Unit switch
        {
            Unit.Point => Value,
            Unit.Percent => Value * referenceLength * 0.01f,
            _ => float.NaN
        };

        return float.IsNaN(value) ? defaultValue : value;
    }

    public float ResolveOrMax(float referenceLength, float minValue)
    {
        var value = Resolve(referenceLength);

        if (!float.IsNaN(value) && !float.IsNaN(minValue))
            return MathF.Max(value, minValue);

        return float.IsNaN(value) ? minValue : value;
    }

    public override string ToString()
    {
        return Unit switch
        {
            Unit.Auto => "Auto",
            Unit.Point => $"{Value.ToString("0", CultureInfo.InvariantCulture)}px",
            Unit.Percent => $"{Value.ToString("0", CultureInfo.InvariantCulture)}%",
            _ => "Undefined"
        };
    }

    public static bool operator ==(StyleValue a, StyleValue b)
        => a.Unit == b.Unit && a.Value.Equals(b.Value);

    public static bool operator !=(StyleValue a, StyleValue b)
        => !(a == b);
}
