using HaselCommon.Utils;

namespace HaselCommon.Tests.Gui;

#pragma warning disable CS1718 // Comparison made to same variable

public class FloatOptionalTest
{
    private const float Empty = float.NaN;
    private const float Zero = 0.0f;
    private const float One = 1.0f;
    private const float Positive = 1234.5f;
    private const float Negative = -9876.5f;

    [Fact]
    public void Value()
    {
        Assert.True(float.IsNaN(Empty));
        Assert.Equal(Zero, 0.0f);
        Assert.Equal(One, 1.0f);
        Assert.Equal(Positive, 1234.5f);
        Assert.Equal(Negative, -9876.5f);

        Assert.True(float.IsNaN(Empty));
        Assert.False(float.IsNaN(Zero));
        Assert.False(float.IsNaN(One));
        Assert.False(float.IsNaN(Positive));
        Assert.False(float.IsNaN(Negative));
    }

    [Fact]
    public void Equality()
    {
        //Assert.True(Empty == Empty);
        //Assert.True(Empty == float.NaN);
        Assert.False(Empty == Zero);
        Assert.False(Empty == Negative);
        Assert.False(Empty == 12.3f);

        Assert.True(Zero == Zero);
        Assert.True(Zero == 0.0f);
        Assert.False(Zero == Positive);
        Assert.False(Zero == -5555.5f);

        Assert.True(One == One);
        Assert.True(One == 1.0f);
        Assert.False(One == Positive);

        Assert.True(Positive == Positive);
        Assert.True(Positive == Positive);
        Assert.False(Positive == One);

        Assert.True(Negative == Negative);
        Assert.True(Negative == Negative);
        Assert.False(Negative == Zero);
    }

    [Fact]
    public void Inequality()
    {
        //Assert.False(Empty != Empty);
        //Assert.False(Empty != float.NaN);
        Assert.True(Empty != Zero);
        Assert.True(Empty != Negative);
        Assert.True(Empty != 12.3f);

        Assert.False(Zero != Zero);
        Assert.False(Zero != 0.0f);
        Assert.True(Zero != Positive);
        Assert.True(Zero != -5555.5f);

        Assert.False(One != One);
        Assert.False(One != 1.0f);
        Assert.True(One != Positive);

        Assert.False(Positive != Positive);
        Assert.False(Positive != Positive);
        Assert.True(Positive != One);

        Assert.False(Negative != Negative);
        Assert.False(Negative != Negative);
        Assert.True(Negative != Zero);
    }

    [Fact]
    public void Greater_than_with_undefined()
    {
        Assert.False(Empty > Empty);
        Assert.False(Empty > Zero);
        Assert.False(Empty > One);
        Assert.False(Empty > Positive);
        Assert.False(Empty > Negative);
        Assert.False(Zero > Empty);
        Assert.False(One > Empty);
        Assert.False(Positive > Empty);
        Assert.False(Negative > Empty);
    }

    [Fact]
    public void Greater_than()
    {
        Assert.True(Zero > Negative);
        Assert.False(Zero > Zero);
        Assert.False(Zero > Positive);
        Assert.False(Zero > One);

        Assert.True(One > Negative);
        Assert.True(One > Zero);
        Assert.False(One > Positive);

        Assert.True(Negative > float.NegativeInfinity);
    }

    [Fact]
    public void Less_than_with_undefined()
    {
        Assert.False(Empty < Empty);
        Assert.False(Zero < Empty);
        Assert.False(One < Empty);
        Assert.False(Positive < Empty);
        Assert.False(Negative < Empty);
        Assert.False(Empty < Zero);
        Assert.False(Empty < One);
        Assert.False(Empty < Positive);
        Assert.False(Empty < Negative);
    }

    [Fact]
    public void Less_than()
    {
        Assert.True(Negative < Zero);
        Assert.False(Zero < Zero);
        Assert.False(Positive < Zero);
        Assert.False(One < Zero);

        Assert.True(Negative < One);
        Assert.True(Zero < One);
        Assert.False(Positive < One);

        Assert.True(float.NegativeInfinity < Negative);
    }

    [Fact]
    public void Greater_than_equals_with_undefined()
    {
        //Assert.True(Empty >= Empty);
        Assert.False(Empty >= Zero);
        Assert.False(Empty >= One);
        Assert.False(Empty >= Positive);
        Assert.False(Empty >= Negative);
        Assert.False(Zero >= Empty);
        Assert.False(One >= Empty);
        Assert.False(Positive >= Empty);
        Assert.False(Negative >= Empty);
    }

    [Fact]
    public void Greater_than_equals()
    {
        Assert.True(Zero >= Negative);
        Assert.True(Zero >= Zero);
        Assert.False(Zero >= Positive);
        Assert.False(Zero >= One);

        Assert.True(One >= Negative);
        Assert.True(One >= Zero);
        Assert.False(One >= Positive);

        Assert.True(Negative >= float.NegativeInfinity);
    }

    [Fact]
    public void Less_than_equals_with_undefined()
    {
        //Assert.True(Empty <= Empty);
        Assert.False(Zero <= Empty);
        Assert.False(One <= Empty);
        Assert.False(Positive <= Empty);
        Assert.False(Negative <= Empty);
        Assert.False(Empty <= Zero);
        Assert.False(Empty <= One);
        Assert.False(Empty <= Positive);
        Assert.False(Empty <= Negative);
    }

    [Fact]
    public void Less_than_equals()
    {
        Assert.True(Negative <= Zero);
        Assert.True(Zero <= Zero);
        Assert.False(Positive <= Zero);
        Assert.False(One <= Zero);

        Assert.True(Negative <= One);
        Assert.True(Zero <= One);
        Assert.False(Positive <= One);

        Assert.True(float.NegativeInfinity <= Negative);
    }

    [Fact]
    public void Addition()
    {
        var n = Negative;
        var p = Positive;

        Assert.Equal(Zero + One, One);
        Assert.Equal(Negative + Positive, n + p);
        Assert.Equal(Empty + Zero, Empty);
        Assert.Equal(Empty + Empty, Empty);
        Assert.Equal(Negative + Empty, Empty);
    }

    [Fact]
    public void MaxOrDefined()
    {
        Assert.Equal(Empty, MathUtils.MaxOrDefined(Empty, Empty));
        Assert.Equal(Positive, MathUtils.MaxOrDefined(Empty, Positive));
        Assert.Equal(Negative, MathUtils.MaxOrDefined(Negative, Empty));
        Assert.Equal(Negative, MathUtils.MaxOrDefined(Negative, float.NegativeInfinity));
        Assert.Equal(1.125f, MathUtils.MaxOrDefined(1.0f, 1.125f));
    }

    [Fact]
    public void Unwrap()
    {
        Assert.True(float.IsNaN(Empty));
        Assert.Equal(Zero, 0.0f);
        Assert.Equal(123456.78f, 123456.78f);
    }
}
