using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;

namespace HaselCommon.Gui;

public partial class Node
{
    private static Align ResolveChildAlignment(Node node, Node child)
    {
        var align = child.AlignSelf == Align.Auto
            ? node.AlignItems
            : child.AlignSelf;

        if (align == Align.Baseline && node.FlexDirection.IsColumn())
        {
            return Align.FlexStart;
        }

        return align;
    }

    /**
     * Fallback alignment to use on overflow
     * https://www.w3.org/TR/css-align-3/#distribution-values
     */
    private static Align FallbackAlignment(Align align)
    {
        return align switch
        {
            // Fallback to flex-start
            Align.SpaceBetween or Align.Stretch => Align.FlexStart,
            // Fallback to safe center. TODO: This should be aligned to Start
            // instead of FlexStart (for row-reverse containers)
            Align.SpaceAround or Align.SpaceEvenly => Align.FlexStart,
            _ => align,
        };
    }

    /**
     * Fallback alignment to use on overflow
     * https://www.w3.org/TR/css-align-3/#distribution-values
     */
    private static Justify FallbackAlignment(Justify align)
    {
        return align switch
        {
            // Fallback to flex-start
            // TODO: Support `justify-content: stretch`
            // case Justify.Stretch:
            Justify.SpaceBetween => Justify.FlexStart,
            // Fallback to safe center. TODO: This should be aligned to Start
            // instead of FlexStart (for row-reverse containers)
            Justify.SpaceAround or Justify.SpaceEvenly => Justify.FlexStart,
            _ => align,
        };
    }
}
