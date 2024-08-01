namespace HaselCommon.Yoga;

public enum Align
{
    Auto,
    FlexStart,
    Center,
    FlexEnd,
    Stretch,
    Baseline,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

public enum Dimension
{
    Width,
    Height
}

public enum Direction
{
    Inherit,
    LTR,
    RTL
}

public enum Display
{
    Flex,
    None
}

public enum Edge
{
    Left,
    Top,
    Right,
    Bottom,
    Start,
    End,
    Horizontal,
    Vertical,
    All
}

public enum Errata
{
    None = 0,
    StretchFlexBasis = 1,
    AbsolutePositioningIncorrect = 2,
    AbsolutePercentAgainstInnerSize = 4,
    All = int.MaxValue,
    Classic = int.MaxValue - 1
}

public enum ExperimentalFeature
{
    WebFlexBasis
}

public enum FlexDirection
{
    Column,
    ColumnReverse,
    Row,
    RowReverse
}

public enum Gutter
{
    Column,
    Row,
    All
}

public enum Justify
{
    FlexStart,
    Center,
    FlexEnd,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

public enum LogLevel
{
    Error,
    Warn,
    Info,
    Debug,
    Verbose,
    Fatal
}

public enum MeasureMode
{
    Undefined,
    Exactly,
    AtMost
}

public enum NodeType
{
    Default,
    Text
}

public enum Overflow
{
    Visible,
    Hidden,
    Scroll
}

public enum PhysicalEdge
{
    Left,
    Top,
    Right,
    Bottom
}

public enum PositionType
{
    Static,
    Relative,
    Absolute
}

public enum Unit
{
    Undefined,
    Point,
    Percent,
    Auto
}

public enum Wrap
{
    NoWrap,
    Wrap,
    WrapReverse
}
