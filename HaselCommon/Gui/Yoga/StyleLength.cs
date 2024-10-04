using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using HaselCommon.Extensions.Math;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Gui.Yoga;

public struct StyleLength
{
    private float _value;
    public Unit Unit;

    public float Value
    {
        get => IsUndefined || IsAuto ? float.NaN : _value;
        set => _value = value;
    }

    public static StyleLength Undefined
        => new() { Value = float.NaN, Unit = Unit.Undefined };

    public static StyleLength Auto
        => new() { Value = float.NaN, Unit = Unit.Auto };

    public static StyleLength Percent(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || float.IsNegativeInfinity(value))
            return Undefined;

        return new() { Value = value, Unit = Unit.Percent };
    }

    public static StyleLength Point(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || float.IsNegativeInfinity(value))
            return Undefined;

        return new() { Value = value, Unit = Unit.Point };
    }

    public static implicit operator StyleLength(float value) => Point(value);

    public bool IsAuto
        => Unit is Unit.Auto;

    public bool IsUndefined
        => Unit is Unit.Undefined;

    public bool IsDefined
        => !IsUndefined;

    public bool IsApproximately(StyleLength other)
        => Unit == other.Unit && Value.IsApproximately(other.Value);

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

    public static bool operator ==(StyleLength? a, StyleLength? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(StyleLength? a, StyleLength? b)
        => !(a == b);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not StyleLength other)
            return false;

        return Unit == other.Unit && (Unit is Unit.Undefined or Unit.Auto || Value == other.Value);
    }

    public override int GetHashCode()
        => HashCode.Combine(Value, Unit);
}
