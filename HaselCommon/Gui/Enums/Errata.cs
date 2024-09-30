namespace HaselCommon.Gui.Enums;

public enum Errata
{
    None = 0,
    StretchFlexBasis = 1,
    AbsolutePositioningIncorrect = 2,
    AbsolutePercentAgainstInnerSize = 4,
    All = StretchFlexBasis | AbsolutePositioningIncorrect | AbsolutePercentAgainstInnerSize,
    Classic = StretchFlexBasis,
}
