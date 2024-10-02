using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui;

internal class LayoutResults
{
    internal const int MaxCachedMeasurements = 8;

    internal uint ComputedFlexBasisGeneration = 0;
    internal float ComputedFlexBasis = float.NaN;

    // Instead of recomputing the entire layout every single time, we cache some
    // information to break early when nothing changed
    internal uint GenerationCount = 0;
    internal uint ConfigVersion = 0;
    internal Direction LastOwnerDirection = Direction.Inherit;
    internal uint NextCachedMeasurementsIndex = 0;
    internal CachedMeasurement[] CachedMeasurements = new CachedMeasurement[MaxCachedMeasurements];
    internal CachedMeasurement CachedLayout = new();

    internal Direction Direction = Direction.Inherit;
    internal bool HadOverflow = false;

    internal readonly float[] Dimensions = [float.NaN, float.NaN];
    internal readonly float[] MeasuredDimensions = new float[2];
    internal readonly float[] Position = new float[4];
    internal readonly float[] Margin = new float[4];
    internal readonly float[] Border = new float[4];
    internal readonly float[] Padding = new float[4];

    public static bool operator ==(LayoutResults? a, LayoutResults? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(LayoutResults? a, LayoutResults? b)
        => !(a == b);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not LayoutResults other)
            return false;

        var isEqual = Direction == other.Direction &&
            HadOverflow == other.HadOverflow &&
            LastOwnerDirection == other.LastOwnerDirection &&
            ConfigVersion == other.ConfigVersion &&
            NextCachedMeasurementsIndex == other.NextCachedMeasurementsIndex &&
            CachedLayout == other.CachedLayout &&
            ComputedFlexBasis == other.ComputedFlexBasis;

        for (var i = 0; i < Position.Length; i++)
            isEqual = isEqual && Position[i] == other.Position[i];

        for (var i = 0; i < Dimensions.Length; i++)
            isEqual = isEqual && Dimensions[i] == other.Dimensions[i];

        for (var i = 0; i < MeasuredDimensions.Length; i++)
            isEqual = isEqual && MeasuredDimensions[i] == other.MeasuredDimensions[i];

        for (var i = 0; i < Margin.Length; i++)
            isEqual = isEqual && Margin[i] == other.Margin[i];

        for (var i = 0; i < Border.Length; i++)
            isEqual = isEqual && Border[i] == other.Border[i];

        for (var i = 0; i < Padding.Length; i++)
            isEqual = isEqual && Padding[i] == other.Padding[i];

        for (var i = 0; i < MaxCachedMeasurements; i++)
            isEqual = isEqual && CachedMeasurements[i] == other.CachedMeasurements[i];

        return isEqual;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ComputedFlexBasisGeneration);
        hash.Add(ComputedFlexBasis);
        hash.Add(GenerationCount);
        hash.Add(ConfigVersion);
        hash.Add(LastOwnerDirection);
        hash.Add(NextCachedMeasurementsIndex);
        hash.Add(CachedMeasurements);
        hash.Add(CachedLayout);
        hash.Add(Direction);
        hash.Add(HadOverflow);

        for (var i = 0; i < Position.Length; i++)
            hash.Add(Position[i]);

        for (var i = 0; i < Dimensions.Length; i++)
            hash.Add(Dimensions[i]);

        for (var i = 0; i < MeasuredDimensions.Length; i++)
            hash.Add(MeasuredDimensions[i]);

        for (var i = 0; i < Margin.Length; i++)
            hash.Add(Margin[i]);

        for (var i = 0; i < Border.Length; i++)
            hash.Add(Border[i]);

        for (var i = 0; i < Padding.Length; i++)
            hash.Add(Padding[i]);

        return hash.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float GetDimension(Dimension dimension)
    {
        return Dimensions[(int)dimension];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetDimension(float value, Dimension dimension)
    {
        Dimensions[(int)dimension] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float GetMeasuredDimension(Dimension dimension)
    {
        return MeasuredDimensions[(int)dimension];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetMeasuredDimension(float value, Dimension dimension)
    {
        MeasuredDimensions[(int)dimension] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float GetPosition(PhysicalEdge edge)
    {
        return Position[(int)edge];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetPosition(float value, PhysicalEdge edge)
    {
        Position[(int)edge] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float GetMargin(PhysicalEdge edge)
    {
        return Margin[(int)edge];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetMargin(float value, PhysicalEdge edge)
    {
        Margin[(int)edge] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float GetBorder(PhysicalEdge edge)
    {
        return Border[(int)edge];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetBorder(float value, PhysicalEdge edge)
    {
        Border[(int)edge] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float GetPadding(PhysicalEdge edge)
    {
        return Padding[(int)edge];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetPadding(float value, PhysicalEdge edge)
    {
        Padding[(int)edge] = value;
    }
}
