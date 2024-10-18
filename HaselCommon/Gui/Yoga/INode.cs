using System.Collections.Generic;
using System.Numerics;
using HaselCommon.Gui.Yoga.Events;
using YogaSharp;

namespace HaselCommon.Gui.Yoga;

public interface INode : IList<Node>, IDisposable
{
    Guid Guid { get; }
    string TypeName { get; }

    void Draw();
    void DrawChildNodes();
    void Update();
    void UpdateChildNodes();

    float Baseline(float width, float height);
    Vector2 Measure(float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode);

    void DrawContent();
    void UpdateContent();

    #region Interop: Node

    YGNodeType NodeType { get; set; }
    bool AlwaysFormsContainingBlock { get; set; }
    bool IsReferenceBaseline { get; set; }
    bool HasBaselineFunc { get; }
    bool HasMeasureFunc { get; }
    bool HasNewLayout { get; set; }
    bool IsDirty { get; set; }

    #endregion

    #region Interop: Style

    YGDirection Direction { get; set; }
    YGFlexDirection FlexDirection { get; set; }
    YGJustify JustifyContent { get; set; }
    YGAlign AlignContent { get; set; }
    YGAlign AlignItems { get; set; }
    YGAlign AlignSelf { get; set; }
    YGPositionType PositionType { get; set; }
    YGWrap FlexWrap { get; set; }
    YGOverflow Overflow { get; set; }
    YGDisplay Display { get; set; }
    float Flex { get; set; }
    float FlexGrow { get; set; }
    float FlexShrink { get; set; }
    YGValue FlexBasis { get; set; }
    YGValue Margin { get; set; }
    YGValue MarginTop { get; set; }
    YGValue MarginBottom { get; set; }
    YGValue MarginLeft { get; set; }
    YGValue MarginRight { get; set; }
    YGValue MarginHorizontal { get; set; }
    YGValue MarginVertical { get; set; }
    YGValue Position { get; set; }
    YGValue PositionTop { get; set; }
    YGValue PositionBottom { get; set; }
    YGValue PositionLeft { get; set; }
    YGValue PositionRight { get; set; }
    YGValue PositionHorizontal { get; set; }
    YGValue PositionVertical { get; set; }
    YGValue PositionStart { get; set; }
    YGValue PositionEnd { get; set; }
    YGValue Padding { get; set; }
    YGValue PaddingTop { get; set; }
    YGValue PaddingBottom { get; set; }
    YGValue PaddingLeft { get; set; }
    YGValue PaddingRight { get; set; }
    YGValue PaddingHorizontal { get; set; }
    YGValue PaddingVertical { get; set; }
    YGValue PaddingStart { get; set; }
    YGValue PaddingEnd { get; set; }
    float Border { get; set; }
    float BorderTop { get; set; }
    float BorderBottom { get; set; }
    float BorderLeft { get; set; }
    float BorderRight { get; set; }
    float BorderHorizontal { get; set; }
    float BorderVertical { get; set; }
    float BorderStart { get; set; }
    float BorderEnd { get; set; }
    float Gap { get; set; }
    float RowGap { get; set; }
    float ColumnGap { get; set; }
    YGValue Width { get; set; }
    YGValue Height { get; set; }
    YGValue MinWidth { get; set; }
    YGValue MinHeight { get; set; }
    YGValue MaxWidth { get; set; }
    YGValue MaxHeight { get; set; }
    float AspectRatio { get; set; }

    #endregion

    #region Interop: Layout

    bool HadOverflow { get; }
    YGDirection ComputedDirection { get; }
    float ComputedLeft { get; }
    float ComputedTop { get; }
    float ComputedRight { get; }
    float ComputedBottom { get; }
    float ComputedWidth { get; }
    float ComputedHeight { get; }
    float ComputedMarginTop { get; }
    float ComputedMarginBottom { get; }
    float ComputedMarginLeft { get; }
    float ComputedMarginRight { get; }
    float ComputedBorderTop { get; }
    float ComputedBorderBottom { get; }
    float ComputedBorderLeft { get; }
    float ComputedBorderRight { get; }
    float ComputedPaddingTop { get; }
    float ComputedPaddingBottom { get; }
    float ComputedPaddingLeft { get; }
    float ComputedPaddingRight { get; }

    #endregion

    #region Interop: Custom API

    bool EnableBaselineFunc { get; set; }
    bool EnableMeasureFunc { get; set; }
    Vector2 ComputedPosition { get; }
    Vector2 ComputedSize { get; }
    Vector2 AbsolutePosition { get; }

    void CalculateLayout(Vector2 ownerSize, YGDirection ownerDirection = YGDirection.LTR);

    #endregion

    #region Debug

    bool DebugHasClosingTag { get; }
    string DebugNodeOpenTag { get; }

    #endregion

    #region Events

    Node AddEventListener<T>(Node.EventHandler<T> callback) where T : YogaEvent;
    void DispatchEvent<T>(T eventArgs) where T : YogaEvent;
    void ProcessEvent(object? sender, YogaEvent evt);
    Node RemoveEventListener<T>() where T : YogaEvent;
    Node RemoveEventListener<T>(Node.EventHandler<T> callback) where T : YogaEvent;

    #endregion

    #region Hierarchy

    Node? Parent { get; set; }
    List<Node> Children { get; set; }
    Node? FirstChild { get; }
    Node? LastChild { get; }
    Node? PreviousSibling { get; }
    IReadOnlyList<Node> PreviousSiblings { get; }
    Node? NextSibling { get; }
    IReadOnlyList<Node> NextSiblings { get; }

    void Add(params Node[] children);
    void Traverse(Action<Node> callback);

    #endregion
}
