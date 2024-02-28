using System.Runtime.InteropServices;

namespace HaselCommon.Yoga;

public static unsafe partial class Interop
{
    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeCopyStyle(YGNode* dstNode, YGNode* srcNode);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetDirection(YGNode* node, Direction value);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Direction YGNodeStyleGetDirection(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetFlexDirection(YGNode* node, FlexDirection flexDirection);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern FlexDirection YGNodeStyleGetFlexDirection(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetJustifyContent(YGNode* node, Justify justifyContent);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Justify YGNodeStyleGetJustifyContent(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetAlignContent(YGNode* node, Align alignContent);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Align YGNodeStyleGetAlignContent(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetAlignItems(YGNode* node, Align alignItems);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Align YGNodeStyleGetAlignItems(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetAlignSelf(YGNode* node, Align alignSelf);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Align YGNodeStyleGetAlignSelf(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetPositionType(YGNode* node, PositionType positionType);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern PositionType YGNodeStyleGetPositionType(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetFlexWrap(YGNode* node, Wrap flexWrap);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Wrap YGNodeStyleGetFlexWrap(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetOverflow(YGNode* node, Overflow overflow);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Overflow YGNodeStyleGetOverflow(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetDisplay(YGNode* node, Display display);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Display YGNodeStyleGetDisplay(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetFlex(YGNode* node, float flex);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeStyleGetFlex(YGNode* nodeRef);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetFlexGrow(YGNode* node, float flexGrow);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeStyleGetFlexGrow(YGNode* nodeRef);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetFlexShrink(YGNode* node, float flexShrink);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeStyleGetFlexShrink(YGNode* nodeRef);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetFlexBasis(YGNode* node, float flexBasis);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetFlexBasisPercent(YGNode* node, float flexBasisPercent);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetFlexBasisAuto(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetFlexBasis(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetPosition(YGNode* node, Edge edge, float points);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetPositionPercent(YGNode* node, Edge edge, float percent);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetPosition(YGNode* node, Edge edge);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMargin(YGNode* node, Edge edge, float points);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMarginPercent(YGNode* node, Edge edge, float percent);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMarginAuto(YGNode* node, Edge edge);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetMargin(YGNode* node, Edge edge);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetPadding(YGNode* node, Edge edge, float points);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetPaddingPercent(YGNode* node, Edge edge, float percent);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetPadding(YGNode* node, Edge edge);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetBorder(YGNode* node, Edge edge, float border);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeStyleGetBorder(YGNode* node, Edge edge);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetGap(YGNode* node, Gutter gutter, float gapLength);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeStyleGetGap(YGNode* node, Gutter gutter);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetAspectRatio(YGNode* node, float aspectRatio);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeStyleGetAspectRatio(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetWidth(YGNode* node, float points);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetWidthPercent(YGNode* node, float percent);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetWidthAuto(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetWidth(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetHeight(YGNode* node, float points);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetHeightPercent(YGNode* node, float percent);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetHeightAuto(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetHeight(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMinWidth(YGNode* node, float minWidth);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMinWidthPercent(YGNode* node, float minWidth);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetMinWidth(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMinHeight(YGNode* node, float minHeight);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMinHeightPercent(YGNode* node, float minHeight);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetMinHeight(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMaxWidth(YGNode* node, float maxWidth);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMaxWidthPercent(YGNode* node, float maxWidth);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetMaxWidth(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMaxHeight(YGNode* node, float maxHeight);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void YGNodeStyleSetMaxHeightPercent(YGNode* node, float maxHeight);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern YGValue YGNodeStyleGetMaxHeight(YGNode* node);
}
