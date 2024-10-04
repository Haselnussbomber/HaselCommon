using HaselCommon.Extensions.Math;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Gui.Yoga;

public partial class Node
{
    private static bool SizeIsExactAndMatchesOldMeasuredSize(SizingMode sizeMode, float size, float lastComputedSize)
    {
        return sizeMode == SizingMode.StretchFit && size.IsApproximately(lastComputedSize);
    }

    private static bool OldSizeIsMaxContentAndStillFits(SizingMode sizeMode, float size, SizingMode lastSizeMode, float lastComputedSize)
    {
        return sizeMode == SizingMode.FitContent && lastSizeMode == SizingMode.MaxContent && (size >= lastComputedSize || size.IsApproximately(lastComputedSize));
    }

    private static bool NewSizeIsStricterAndStillValid(SizingMode sizeMode, float size, SizingMode lastSizeMode, float lastSize, float lastComputedSize)
    {
        return lastSizeMode == SizingMode.FitContent &&
            sizeMode == SizingMode.FitContent && !float.IsNaN(lastSize) &&
            !float.IsNaN(size) && !float.IsNaN(lastComputedSize) &&
            lastSize > size &&
            (lastComputedSize <= size || size.IsApproximately(lastComputedSize));
    }

    private bool CanUseCachedMeasurement(
        SizingMode widthMode,
        float availableWidth,
        SizingMode heightMode,
        float availableHeight,
        SizingMode lastWidthMode,
        float lastAvailableWidth,
        SizingMode lastHeightMode,
        float lastAvailableHeight,
        float lastComputedWidth,
        float lastComputedHeight,
        float marginRow,
        float marginColumn)
    {
        if ((!float.IsNaN(lastComputedHeight) && lastComputedHeight < 0) || ((!float.IsNaN(lastComputedWidth)) && lastComputedWidth < 0))
            return false;

        var pointScaleFactor = Config.PointScaleFactor;
        var useRoundedComparison = pointScaleFactor != 0;

        var effectiveWidth = useRoundedComparison
            ? RoundValueToPixelGrid(availableWidth, pointScaleFactor, false, false)
            : availableWidth;
        var effectiveHeight = useRoundedComparison
            ? RoundValueToPixelGrid(availableHeight, pointScaleFactor, false, false)
            : availableHeight;
        var effectiveLastWidth = useRoundedComparison
            ? RoundValueToPixelGrid(lastAvailableWidth, pointScaleFactor, false, false)
            : lastAvailableWidth;
        var effectiveLastHeight = useRoundedComparison
            ? RoundValueToPixelGrid(lastAvailableHeight, pointScaleFactor, false, false)
            : lastAvailableHeight;

        var hasSameWidthSpec = lastWidthMode == widthMode && effectiveLastWidth.IsApproximately(effectiveWidth);
        var hasSameHeightSpec = lastHeightMode == heightMode && effectiveLastHeight.IsApproximately(effectiveHeight);

        var widthIsCompatible =
            hasSameWidthSpec ||
            SizeIsExactAndMatchesOldMeasuredSize(
                widthMode, availableWidth - marginRow, lastComputedWidth) ||
            OldSizeIsMaxContentAndStillFits(
                widthMode,
                availableWidth - marginRow,
                lastWidthMode,
                lastComputedWidth) ||
            NewSizeIsStricterAndStillValid(
                widthMode,
                availableWidth - marginRow,
                lastWidthMode,
                lastAvailableWidth,
                lastComputedWidth);

        var heightIsCompatible =
            hasSameHeightSpec ||
            SizeIsExactAndMatchesOldMeasuredSize(
                heightMode,
                availableHeight - marginColumn,
                lastComputedHeight) ||
            OldSizeIsMaxContentAndStillFits(heightMode,
                availableHeight - marginColumn,
                lastHeightMode,
                lastComputedHeight) ||
            NewSizeIsStricterAndStillValid(heightMode,
                availableHeight - marginColumn,
                lastHeightMode,
                lastAvailableHeight,
                lastComputedHeight);

        return widthIsCompatible && heightIsCompatible;
    }
}
