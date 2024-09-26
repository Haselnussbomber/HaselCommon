using System.Numerics;
using YogaSharp;
using YGMeasureFuncDelegate = YogaSharp.Interop.YGMeasureFuncDelegate;

namespace HaselCommon.ImGuiYoga;

public unsafe partial class Node
{
    internal readonly YGNode* YGNode = YogaSharp.YGNode.New();

    #region YGNode API

    public bool HasNewLayout
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetHasNewLayout();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetHasNewLayout(value);
        }
    }

    public bool IsDirty
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->IsDirty();
        }

        set
        {
            if (value)
            {
                ThrowIfDisposed();
                YGNode->MarkDirty();
            }
        }
    }

    /// <inheritdoc cref="YGNode.Reset()" />
    public void Reset()
    {
        ThrowIfDisposed();
        YGNode->Reset();
        // TODO: reset custom attributes (like border color, border radius...)
    }

    /// <inheritdoc cref="YGNode.SetMeasureFunc(nint)" />
    public void SetMeasureFunc(YGMeasureFuncDelegate measureFuncDelege)
    {
        MeasureFuncHandle = GCHandle.Alloc(measureFuncDelege);
        YGNode->SetMeasureFunc(Marshal.GetFunctionPointerForDelegate(measureFuncDelege));
    }

    public bool HasMeasureFunc
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->HasMeasureFunc();
        }
    }

    /// <inheritdoc cref="YGNode.SetBaselineFunc(nint)" />
    public void SetBaselineFunc(nint baselineFunc)
    {
        ThrowIfDisposed();
        YGNode->SetBaselineFunc(baselineFunc);
    }

    public bool HasBaselineFunc
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->HasBaselineFunc();
        }
    }

    public bool IsReferenceBaseline
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->IsReferenceBaseline();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetIsReferenceBaseline(value);
        }
    }

    public NodeType NodeType
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetNodeType();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetNodeType(value);
        }
    }

    public bool AlwaysFormsContainingBlock
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetAlwaysFormsContainingBlock();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetAlwaysFormsContainingBlock(value);
        }
    }

    public YGDirection Direction
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetDirection();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetDirection(value);
        }
    }

    public YGFlexDirection FlexDirection
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetFlexDirection();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetFlexDirection(value);
        }
    }

    public YGJustify JustifyContent
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetJustifyContent();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetJustifyContent(value);
        }
    }

    public YGAlign AlignContent
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetAlignContent();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetAlignContent(value);
        }
    }

    public YGAlign AlignItems
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetAlignItems();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetAlignItems(value);
        }
    }

    public YGAlign AlignSelf
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetAlignSelf();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetAlignSelf(value);
        }
    }

    public YGPositionType PositionType
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPositionType();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetPositionType(value);
        }
    }

    public YGWrap FlexWrap
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetFlexWrap();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetFlexWrap(value);
        }
    }

    public YGOverflow Overflow
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetOverflow();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetOverflow(value);
        }
    }

    public YGDisplay Display
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetDisplay();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetDisplay(value);
        }
    }

    public float Flex
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetFlex();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetFlex(value);
        }
    }

    public float FlexGrow
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetFlexGrow();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetFlexGrow(value);
        }
    }

    public float FlexShrink
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetFlexShrink();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetFlexShrink(value);
        }
    }

    public YGValue FlexBasis
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetFlexBasis();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetFlexBasisAuto();
                    break;

                case YGUnit.Point:
                    YGNode->SetFlexBasis(value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetFlexBasisPercent(value.value);
                    break;
            }
        }
    }

    public YGValue PositionTop
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPosition(YGEdge.Top);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPosition(YGEdge.Top, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPositionPercent(YGEdge.Top, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public YGValue PositionLeft
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPosition(YGEdge.Left);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPosition(YGEdge.Left, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPositionPercent(YGEdge.Left, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public YGValue Margin
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMargin(YGEdge.All);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMarginAuto(YGEdge.All);
                    break;

                case YGUnit.Point:
                    YGNode->SetMargin(YGEdge.All, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMarginPercent(YGEdge.All, value.value);
                    break;
            }
        }
    }

    public YGValue MarginTop
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMargin(YGEdge.Top);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMarginAuto(YGEdge.Top);
                    break;

                case YGUnit.Point:
                    YGNode->SetMargin(YGEdge.Top, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMarginPercent(YGEdge.Top, value.value);
                    break;
            }
        }
    }

    public YGValue MarginLeft
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMargin(YGEdge.Left);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMarginAuto(YGEdge.Left);
                    break;

                case YGUnit.Point:
                    YGNode->SetMargin(YGEdge.Left, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMarginPercent(YGEdge.Left, value.value);
                    break;
            }
        }
    }

    public YGValue MarginRight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMargin(YGEdge.Right);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMarginAuto(YGEdge.Right);
                    break;

                case YGUnit.Point:
                    YGNode->SetMargin(YGEdge.Right, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMarginPercent(YGEdge.Right, value.value);
                    break;
            }
        }
    }

    public YGValue MarginBottom
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMargin(YGEdge.Bottom);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMarginAuto(YGEdge.Bottom);
                    break;

                case YGUnit.Point:
                    YGNode->SetMargin(YGEdge.Bottom, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMarginPercent(YGEdge.Bottom, value.value);
                    break;
            }
        }
    }

    public YGValue MarginHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMargin(YGEdge.Horizontal);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMarginAuto(YGEdge.Horizontal);
                    break;

                case YGUnit.Point:
                    YGNode->SetMargin(YGEdge.Horizontal, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMarginPercent(YGEdge.Horizontal, value.value);
                    break;
            }
        }
    }

    public YGValue MarginVertical
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMargin(YGEdge.Vertical);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMarginAuto(YGEdge.Vertical);
                    break;

                case YGUnit.Point:
                    YGNode->SetMargin(YGEdge.Vertical, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMarginPercent(YGEdge.Vertical, value.value);
                    break;
            }
        }
    }

    public YGValue Padding
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPadding(YGEdge.All);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPadding(YGEdge.All, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPaddingPercent(YGEdge.All, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public YGValue PaddingTop
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPadding(YGEdge.Top);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPadding(YGEdge.Top, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPaddingPercent(YGEdge.Top, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public YGValue PaddingLeft
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPadding(YGEdge.Left);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPadding(YGEdge.Left, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPaddingPercent(YGEdge.Left, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public YGValue PaddingRight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPadding(YGEdge.Right);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPadding(YGEdge.Right, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPaddingPercent(YGEdge.Right, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public YGValue PaddingBottom
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPadding(YGEdge.Bottom);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPadding(YGEdge.Bottom, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPaddingPercent(YGEdge.Bottom, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public YGValue PaddingHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPadding(YGEdge.Horizontal);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPadding(YGEdge.Horizontal, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPaddingPercent(YGEdge.Horizontal, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public YGValue PaddingVertical
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetPadding(YGEdge.Vertical);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    YGNode->SetPadding(YGEdge.Vertical, value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetPaddingPercent(YGEdge.Vertical, value.value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public float Border
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetBorder(YGEdge.All);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetBorder(YGEdge.All, value);
        }
    }

    public float BorderTop
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetBorder(YGEdge.Top);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetBorder(YGEdge.Top, value);
        }
    }

    public float BorderLeft
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetBorder(YGEdge.Left);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetBorder(YGEdge.Left, value);
        }
    }

    public float BorderRight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetBorder(YGEdge.Right);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetBorder(YGEdge.Right, value);
        }
    }

    public float BorderBottom
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetBorder(YGEdge.Bottom);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetBorder(YGEdge.Bottom, value);
        }
    }

    public float BorderHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetBorder(YGEdge.Horizontal);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetBorder(YGEdge.Horizontal, value);
        }
    }

    public float BorderVertical
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetBorder(YGEdge.Vertical);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetBorder(YGEdge.Vertical, value);
        }
    }

    public float Gap
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetGap(YGGutter.All);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetGap(YGGutter.All, value);
        }
    }

    public float GapColumn
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetGap(YGGutter.Column);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetGap(YGGutter.Column, value);
        }
    }

    public float GapRow
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetGap(YGGutter.Row);
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetGap(YGGutter.Row, value);
        }
    }

    public float AspectRatio
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetAspectRatio();
        }

        set
        {
            ThrowIfDisposed();
            YGNode->SetAspectRatio(value);
        }
    }

    public YGValue Width
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetWidth();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetWidthAuto();
                    break;

                case YGUnit.Point:
                    YGNode->SetWidth(value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetWidthPercent(value.value);
                    break;
            }
        }
    }

    public YGValue Height
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetHeight();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetHeightAuto();
                    break;

                case YGUnit.Point:
                    YGNode->SetHeight(value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetHeightPercent(value.value);
                    break;
            }
        }
    }

    public YGValue MinWidth
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMinWidth();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMinWidth(float.NaN);
                    break;

                case YGUnit.Point:
                    YGNode->SetMinWidth(value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMinWidthPercent(value.value);
                    break;
            }
        }
    }

    public YGValue MinHeight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMinHeight();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMinHeight(float.NaN);
                    break;

                case YGUnit.Point:
                    YGNode->SetMinHeight(value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMinHeightPercent(value.value);
                    break;
            }
        }
    }

    public YGValue MaxWidth
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMaxWidth();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMaxWidth(float.NaN);
                    break;

                case YGUnit.Point:
                    YGNode->SetMaxWidth(value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMaxWidthPercent(value.value);
                    break;
            }
        }
    }

    public YGValue MaxHeight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetMaxHeight();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    YGNode->SetMaxHeight(float.NaN);
                    break;

                case YGUnit.Point:
                    YGNode->SetMaxHeight(value.value);
                    break;

                case YGUnit.Percent:
                    YGNode->SetMaxHeightPercent(value.value);
                    break;
            }
        }
    }

    public float ComputedLeft
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedLeft();
        }
    }

    public float ComputedTop
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedTop();
        }
    }

    public float ComputedRight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedRight();
        }
    }

    public float ComputedBottom
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedBottom();
        }
    }

    public float ComputedWidth
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedWidth();
        }
    }

    public float ComputedHeight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedHeight();
        }
    }

    public YGDirection ComputedDirection
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedDirection();
        }
    }

    public bool HadOverflow
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetHadOverflow();
        }
    }

    public float ComputedMargin
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedMargin(YGEdge.All);
        }
    }

    public float ComputedMarginTop
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedMargin(YGEdge.Top);
        }
    }

    public float ComputedMarginRight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedMargin(YGEdge.Right);
        }
    }

    public float ComputedMarginBottom
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedMargin(YGEdge.Bottom);
        }
    }

    public float ComputedMarginLeft
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedMargin(YGEdge.Left);
        }
    }

    public float ComputedMarginHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedMargin(YGEdge.Horizontal);
        }
    }

    public float ComputedMarginVertical
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedMargin(YGEdge.Vertical);
        }
    }

    public float ComputedPadding
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedPadding(YGEdge.All);
        }
    }

    public float ComputedPaddingTop
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedPadding(YGEdge.Top);
        }
    }

    public float ComputedPaddingRight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedPadding(YGEdge.Right);
        }
    }

    public float ComputedPaddingBottom
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedPadding(YGEdge.Bottom);
        }
    }

    public float ComputedPaddingLeft
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedPadding(YGEdge.Left);
        }
    }

    public float ComputedPaddingHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedPadding(YGEdge.Horizontal);
        }
    }

    public float ComputedPaddingVertical
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedPadding(YGEdge.Vertical);
        }
    }

    public float ComputedBorder
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedBorder(YGEdge.All);
        }
    }

    public float ComputedBorderTop
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedBorder(YGEdge.Top);
        }
    }

    public float ComputedBorderRight
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedBorder(YGEdge.Right);
        }
    }

    public float ComputedBorderBottom
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedBorder(YGEdge.Bottom);
        }
    }

    public float ComputedBorderLeft
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedBorder(YGEdge.Left);
        }
    }

    public float ComputedBorderHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedBorder(YGEdge.Horizontal);
        }
    }

    public float ComputedBorderVertical
    {
        get
        {
            ThrowIfDisposed();
            return YGNode->GetComputedBorder(YGEdge.Vertical);
        }
    }

    /// <inheritdoc cref="YGNode.CopyStyle(YGNode*)" />
    public void CopyStyle(YGNode* srcNode)
    {
        ThrowIfDisposed();
        YGNode->CopyStyle(srcNode);
    }

    #endregion

    #region Custom YGNode API

    public void CalculateLayout(Vector2 ownerSize, YGDirection ownerDirection = YGDirection.LTR)
    {
        ThrowIfDisposed();
        YGNode->CalculateLayout(ownerSize.X, ownerSize.Y, ownerDirection);
    }

    public Vector2 ComputedSize
    {
        get
        {
            ThrowIfDisposed();
            return new(YGNode->GetComputedWidth(), YGNode->GetComputedHeight());
        }
    }

    public Vector2 ComputedPosition
    {
        get
        {
            ThrowIfDisposed();
            return new(YGNode->GetComputedLeft(), YGNode->GetComputedTop());
        }
    }

    public Vector2 CumulativePosition
    {
        get
        {
            ThrowIfDisposed();

            var position = ComputedPosition;

            var parent = YGNode;
            while ((parent = parent->GetOwner()) != null && parent->GetOverflow() != YGOverflow.Scroll)
                position += new Vector2(parent->GetComputedLeft(), parent->GetComputedTop());

            return position;
        }
    }

    #endregion
}
