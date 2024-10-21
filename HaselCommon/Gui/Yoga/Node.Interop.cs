using System.Collections.Concurrent;
using System.Numerics;
using HaselCommon.Gui.Yoga.Attributes;
using YogaSharp;

namespace HaselCommon.Gui.Yoga;

public unsafe partial class Node
{
    private static readonly ConcurrentDictionary<nint, Node> NodeRegistry = [];
    private readonly YGNode* _yogaNode = YGNode.New();

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    #region Node

    /// <inheritdoc cref="YGNode.GetNodeType" />
    [NodeProp("Node")]
    public YGNodeType NodeType
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetNodeType();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetNodeType(value);
        }
    }

    /// <inheritdoc cref="YGNode.GetAlwaysFormsContainingBlock" />
    [NodeProp("Node")]
    public bool AlwaysFormsContainingBlock
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetAlwaysFormsContainingBlock();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetAlwaysFormsContainingBlock(value);
        }
    }

    /// <inheritdoc cref="YGNode.IsReferenceBaseline" />
    [NodeProp("Node")]
    public bool IsReferenceBaseline
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->IsReferenceBaseline();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetIsReferenceBaseline(value);
        }
    }

    [NodeProp("Node")]
    /// <inheritdoc cref="YGNode.HasBaselineFunc" />
    public bool HasBaselineFunc
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->HasBaselineFunc();
        }
    }

    /// <inheritdoc cref="YGNode.HasMeasureFunc" />
    [NodeProp("Node")]
    public bool HasMeasureFunc
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->HasMeasureFunc();
        }
    }

    /// <inheritdoc cref="YGNode.GetHasNewLayout" />
    [NodeProp("Node")]
    public bool HasNewLayout
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetHasNewLayout();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetHasNewLayout(value);
        }
    }

    /// <inheritdoc cref="YGNode.IsDirty" />
    [NodeProp("Node")]
    public bool IsDirty
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->IsDirty();
        }

        set
        {
            if (value)
            {
                ThrowIfDisposed();
                _yogaNode->MarkDirty();
            }
        }
    }


    /*
    /// <inheritdoc cref="YGNode.Reset()" />
    public void Reset()
    {
        ThrowIfDisposed();
        YGNode->Reset();
    }
    */

    #endregion

    #region Style

    [NodeProp("Style", editable: true)]
    public YGDirection Direction
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetDirection();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetDirection(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGFlexDirection FlexDirection
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetFlexDirection();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetFlexDirection(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGJustify JustifyContent
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetJustifyContent();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetJustifyContent(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGAlign AlignContent
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetAlignContent();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetAlignContent(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGAlign AlignItems
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetAlignItems();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetAlignItems(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGAlign AlignSelf
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetAlignSelf();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetAlignSelf(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGPositionType PositionType
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPositionType();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetPositionType(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGWrap FlexWrap
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetFlexWrap();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetFlexWrap(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGOverflow Overflow
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetOverflow();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetOverflow(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGDisplay Display
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetDisplay();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetDisplay(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float Flex
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetFlex();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetFlex(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float FlexGrow
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetFlexGrow();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetFlexGrow(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float FlexShrink
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetFlexShrink();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetFlexShrink(value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue FlexBasis
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetFlexBasis();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetFlexBasis(float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetFlexBasisAuto();
                    break;

                case YGUnit.Point:
                    _yogaNode->SetFlexBasis(value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetFlexBasisPercent(value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue Margin
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMargin(YGEdge.All);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetMargin(YGEdge.All, float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetMarginAuto(YGEdge.All);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMargin(YGEdge.All, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMarginPercent(YGEdge.All, value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MarginTop
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMargin(YGEdge.Top);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetMargin(YGEdge.Top, float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetMarginAuto(YGEdge.Top);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMargin(YGEdge.Top, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMarginPercent(YGEdge.Top, value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MarginBottom
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMargin(YGEdge.Bottom);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetMargin(YGEdge.Bottom, float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetMarginAuto(YGEdge.Bottom);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMargin(YGEdge.Bottom, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMarginPercent(YGEdge.Bottom, value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MarginLeft
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMargin(YGEdge.Left);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetMargin(YGEdge.Left, float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetMarginAuto(YGEdge.Left);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMargin(YGEdge.Left, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMarginPercent(YGEdge.Left, value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MarginRight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMargin(YGEdge.Right);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetMargin(YGEdge.Right, float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetMarginAuto(YGEdge.Right);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMargin(YGEdge.Right, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMarginPercent(YGEdge.Right, value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MarginHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMargin(YGEdge.Horizontal);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetMargin(YGEdge.Horizontal, float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetMarginAuto(YGEdge.Horizontal);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMargin(YGEdge.Horizontal, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMarginPercent(YGEdge.Horizontal, value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MarginVertical
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMargin(YGEdge.Vertical);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetMargin(YGEdge.Vertical, float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetMarginAuto(YGEdge.Vertical);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMargin(YGEdge.Vertical, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMarginPercent(YGEdge.Vertical, value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue Position
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.All);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.All, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.All, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.All, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PositionTop
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.Top);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.Top, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.Top, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.Top, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PositionBottom
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.Bottom);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.Bottom, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.Bottom, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.Bottom, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PositionLeft
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.Left);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.Left, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.Left, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.Left, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PositionRight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.Right);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.Right, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.Right, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.Right, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PositionHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.Horizontal);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.Horizontal, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.Horizontal, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.Horizontal, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PositionVertical
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.Vertical);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.Vertical, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.Vertical, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.Vertical, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PositionStart
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.Start);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.Start, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.Start, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.Start, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PositionEnd
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPosition(YGEdge.End);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPosition(YGEdge.End, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPosition(YGEdge.End, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPositionPercent(YGEdge.End, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue Padding
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.All);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.All, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.All, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.All, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PaddingTop
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.Top);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.Top, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.Top, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.Top, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PaddingBottom
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.Bottom);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.Bottom, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.Bottom, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.Bottom, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PaddingLeft
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.Left);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.Left, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.Left, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.Left, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PaddingRight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.Right);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.Right, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.Right, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.Right, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PaddingHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.Horizontal);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.Horizontal, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.Horizontal, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.Horizontal, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PaddingVertical
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.Vertical);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.Vertical, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.Vertical, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.Vertical, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PaddingStart
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.Start);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.Start, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.Start, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.Start, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue PaddingEnd
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetPadding(YGEdge.End);
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetPadding(YGEdge.End, float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetPadding(YGEdge.End, value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetPaddingPercent(YGEdge.End, value.Value);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public float Border
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.All);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.All, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float BorderTop
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.Top);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.Top, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float BorderBottom
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.Bottom);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.Bottom, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float BorderLeft
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.Left);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.Left, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float BorderRight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.Right);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.Right, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float BorderHorizontal
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.Horizontal);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.Horizontal, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float BorderVertical
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.Vertical);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.Vertical, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float BorderStart
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.Start);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.Start, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float BorderEnd
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetBorder(YGEdge.End);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetBorder(YGEdge.End, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float Gap
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetGap(YGGutter.All);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetGap(YGGutter.All, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float RowGap
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetGap(YGGutter.Row);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetGap(YGGutter.Row, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public float ColumnGap
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetGap(YGGutter.Column);
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetGap(YGGutter.Column, value);
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue Width
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetWidth();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetWidth(float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetWidthAuto();
                    break;

                case YGUnit.Point:
                    _yogaNode->SetWidth(value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetWidthPercent(value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue Height
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetHeight();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                    _yogaNode->SetHeight(float.NaN);
                    break;

                case YGUnit.Auto:
                    _yogaNode->SetHeightAuto();
                    break;

                case YGUnit.Point:
                    _yogaNode->SetHeight(value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetHeightPercent(value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MinWidth
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMinWidth();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    _yogaNode->SetMinWidth(float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMinWidth(value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMinWidthPercent(value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MinHeight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMinHeight();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    _yogaNode->SetMinHeight(float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMinHeight(value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMinHeightPercent(value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MaxWidth
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMaxWidth();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    _yogaNode->SetMaxWidth(float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMaxWidth(value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMaxWidthPercent(value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public YGValue MaxHeight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetMaxHeight();
        }

        set
        {
            ThrowIfDisposed();

            switch (value.Unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    _yogaNode->SetMaxHeight(float.NaN);
                    break;

                case YGUnit.Point:
                    _yogaNode->SetMaxHeight(value.Value);
                    break;

                case YGUnit.Percent:
                    _yogaNode->SetMaxHeightPercent(value.Value);
                    break;
            }
        }
    }

    [NodeProp("Style", editable: true)]
    public float AspectRatio
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetAspectRatio();
        }

        set
        {
            ThrowIfDisposed();
            _yogaNode->SetAspectRatio(value);
        }
    }

    /*
    /// <inheritdoc cref="YGNode.CopyStyle(YGNode*)" />
    public void CopyStyle(YGNode* srcNode)
    {
        ThrowIfDisposed();
        _yogaNode->CopyStyle(srcNode);
    }
    */

    #endregion

    #region Layout

    [NodeProp("Layout")]
    public bool HadOverflow
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetHadOverflow();
        }
    }

    [NodeProp("Layout")]
    public YGDirection ComputedDirection
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedDirection();
        }
    }

    [NodeProp("Layout")]
    public float ComputedLeft
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedLeft();
        }
    }

    [NodeProp("Layout")]
    public float ComputedTop
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedTop();
        }
    }

    [NodeProp("Layout")]
    public float ComputedRight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedRight();
        }
    }

    [NodeProp("Layout")]
    public float ComputedBottom
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedBottom();
        }
    }

    [NodeProp("Layout")]
    public float ComputedWidth
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedWidth();
        }
    }

    [NodeProp("Layout")]
    public float ComputedHeight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedHeight();
        }
    }

    [NodeProp("Layout")]
    public float ComputedMarginTop
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedMargin(YGEdge.Top);
        }
    }

    [NodeProp("Layout")]
    public float ComputedMarginBottom
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedMargin(YGEdge.Bottom);
        }
    }

    [NodeProp("Layout")]
    public float ComputedMarginLeft
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedMargin(YGEdge.Left);
        }
    }

    [NodeProp("Layout")]
    public float ComputedMarginRight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedMargin(YGEdge.Right);
        }
    }

    [NodeProp("Layout")]
    public float ComputedBorderTop
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedBorder(YGEdge.Top);
        }
    }

    [NodeProp("Layout")]
    public float ComputedBorderBottom
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedBorder(YGEdge.Bottom);
        }
    }

    [NodeProp("Layout")]
    public float ComputedBorderLeft
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedBorder(YGEdge.Left);
        }
    }

    [NodeProp("Layout")]
    public float ComputedBorderRight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedBorder(YGEdge.Right);
        }
    }

    [NodeProp("Layout")]
    public float ComputedPaddingTop
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedPadding(YGEdge.Top);
        }
    }

    [NodeProp("Layout")]
    public float ComputedPaddingBottom
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedPadding(YGEdge.Bottom);
        }
    }

    [NodeProp("Layout")]
    public float ComputedPaddingLeft
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedPadding(YGEdge.Left);
        }
    }

    [NodeProp("Layout")]
    public float ComputedPaddingRight
    {
        get
        {
            ThrowIfDisposed();
            return _yogaNode->GetComputedPadding(YGEdge.Right);
        }
    }

    #endregion

    #region Custom API

    public bool EnableBaselineFunc
    {
        get => HasBaselineFunc;
        set
        {
            if (value)
                _yogaNode->SetBaselineFunc((nint)(delegate* unmanaged<YGNode*, float, float, float>)&BaselineFuncWrapper);
            else
                _yogaNode->SetBaselineFunc(0);
        }
    }

    [UnmanagedCallersOnly]
    private static float BaselineFuncWrapper(YGNode* node, float width, float height)
    {
        var csNode = NodeRegistry[(nint)node];
        return csNode.Baseline(width, height);
    }

    public bool EnableMeasureFunc
    {
        get => HasMeasureFunc;
        set
        {
            if (value)
            {
                if (Count != 0)
                    throw new Exception("Cannot set measure function: Nodes with measure functions cannot have children.");

                _yogaNode->SetMeasureFunc((nint)(delegate* unmanaged<YGNode*, float, YGMeasureMode, float, YGMeasureMode, YGSize>)&MeasureFuncWrapper);
            }
            else
            {
                _yogaNode->SetMeasureFunc(0);
            }
        }
    }

    [UnmanagedCallersOnly]
    private static YGSize MeasureFuncWrapper(YGNode* node, float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        var csNode = NodeRegistry[(nint)node];
        return csNode.Measure(width, widthMode, height, heightMode);
    }

    public void CalculateLayout(float width = float.NaN, float height = float.NaN, YGDirection ownerDirection = YGDirection.LTR)
    {
        ThrowIfDisposed();
        _yogaNode->CalculateLayout(width, height, ownerDirection);
    }

    public void CalculateLayout(Vector2 ownerSize, YGDirection ownerDirection = YGDirection.LTR)
    {
        ThrowIfDisposed();
        _yogaNode->CalculateLayout(ownerSize.X, ownerSize.Y, ownerDirection);
    }

    public Vector2 ComputedSize
    {
        get
        {
            ThrowIfDisposed();
            return new(_yogaNode->GetComputedWidth(), _yogaNode->GetComputedHeight());
        }
    }

    public Vector2 ComputedPosition
    {
        get
        {
            ThrowIfDisposed();
            return new(_yogaNode->GetComputedLeft(), _yogaNode->GetComputedTop());
        }
    }

    public Vector2 AbsolutePosition
    {
        get
        {
            if (PositionType != YGPositionType.Relative)
                return ComputedPosition;

            var position = ComputedPosition;

            if (Parent != null && Parent.Overflow is not (YGOverflow.Scroll or YGOverflow.Hidden))
                position += Parent.AbsolutePosition;

            return position;
        }
    }

    #endregion
}
