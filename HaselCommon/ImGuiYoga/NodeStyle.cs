using System.Linq;
using ExCSS;
using HaselCommon.Enums;
using HaselCommon.ImGuiYoga.Style;
using HaselCommon.Utils;
using Microsoft.Extensions.Logging;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

public unsafe class NodeStyle
{
    private readonly Node OwnerNode;
    public ColorProperty Color { get; init; }
    public CursorProperty Cursor { get; init; }
    public FontNameProperty FontName { get; init; }
    public FontSizeProperty FontSize { get; init; }
    public HaselColor BackgroundColor { get; set; }

    internal NodeStyle(Node ownerNode)
    {
        OwnerNode = ownerNode;

        Color = new(OwnerNode);
        Cursor = new(OwnerNode);
        FontName = new(OwnerNode);
        FontSize = new(OwnerNode);

        Reset();
    }

    internal void Reset()
    {
        // Yoga
        Direction = YGDirection.Inherit;
        FlexDirection = YGFlexDirection.Column;
        JustifyContent = YGJustify.FlexStart;
        AlignContent = YGAlign.Auto;
        AlignItems = YGAlign.Stretch;
        AlignSelf = YGAlign.Auto;
        PositionType = YGPositionType.Relative;
        FlexWrap = YGWrap.NoWrap;
        Overflow = YGOverflow.Visible;
        Display = YGDisplay.Flex;
        Flex = float.NaN;
        FlexGrow = float.NaN;
        FlexShrink = float.NaN;
        FlexBasis = float.NaN;
        PositionTop = float.NaN;
        PositionLeft = float.NaN;
        Margin = float.NaN;
        Padding = float.NaN;
        Border = float.NaN;
        Gap = float.NaN;
        GapColumn = float.NaN;
        GapRow = float.NaN;
        AspectRatio = float.NaN;
        Width = float.NaN;
        Height = float.NaN;
        MinWidth = float.NaN;
        MinHeight = float.NaN;
        MaxWidth = float.NaN;
        MaxHeight = float.NaN;

        // Custom
        BorderColor = new();
        BorderRadius = 0;
        Color.SetInherited();
        Cursor.SetInherited();
        FontName.SetInherited();
        FontSize.SetInherited();
    }

    #region YGNode API

    public YGDirection Direction
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetDirection();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetDirection(value);
        }
    }

    public YGFlexDirection FlexDirection
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetFlexDirection();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetFlexDirection(value);
        }
    }

    public YGJustify JustifyContent
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetJustifyContent();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetJustifyContent(value);
        }
    }

    public YGAlign AlignContent
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetAlignContent();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetAlignContent(value);
        }
    }

    public YGAlign AlignItems
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetAlignItems();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetAlignItems(value);
        }
    }

    public YGAlign AlignSelf
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetAlignSelf();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetAlignSelf(value);
        }
    }

    public YGPositionType PositionType
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPositionType();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetPositionType(value);
        }
    }

    public YGWrap FlexWrap
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetFlexWrap();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetFlexWrap(value);
        }
    }

    public YGOverflow Overflow
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetOverflow();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetOverflow(value);
        }
    }

    public YGDisplay Display
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetDisplay();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetDisplay(value);
        }
    }

    public float Flex
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetFlex();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetFlex(value);
        }
    }

    public float FlexGrow
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetFlexGrow();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetFlexGrow(value);
        }
    }

    public float FlexShrink
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetFlexShrink();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetFlexShrink(value);
        }
    }

    public YGValue FlexBasis
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetFlexBasis();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetFlexBasisAuto();
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetFlexBasis(value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetFlexBasisPercent(value.value);
                    break;
            }
        }
    }

    public YGValue PositionTop
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPosition(YGEdge.Top);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPosition(YGEdge.Top, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPositionPercent(YGEdge.Top, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPosition(YGEdge.Left);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPosition(YGEdge.Left, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPositionPercent(YGEdge.Left, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMargin(YGEdge.All);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMarginAuto(YGEdge.All);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMargin(YGEdge.All, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMarginPercent(YGEdge.All, value.value);
                    break;
            }
        }
    }

    public YGValue MarginTop
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMargin(YGEdge.Top);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMarginAuto(YGEdge.Top);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMargin(YGEdge.Top, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMarginPercent(YGEdge.Top, value.value);
                    break;
            }
        }
    }

    public YGValue MarginLeft
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMargin(YGEdge.Left);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMarginAuto(YGEdge.Left);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMargin(YGEdge.Left, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMarginPercent(YGEdge.Left, value.value);
                    break;
            }
        }
    }

    public YGValue MarginRight
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMargin(YGEdge.Right);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMarginAuto(YGEdge.Right);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMargin(YGEdge.Right, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMarginPercent(YGEdge.Right, value.value);
                    break;
            }
        }
    }

    public YGValue MarginBottom
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMargin(YGEdge.Bottom);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMarginAuto(YGEdge.Bottom);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMargin(YGEdge.Bottom, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMarginPercent(YGEdge.Bottom, value.value);
                    break;
            }
        }
    }

    public YGValue MarginHorizontal
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMargin(YGEdge.Horizontal);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMarginAuto(YGEdge.Horizontal);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMargin(YGEdge.Horizontal, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMarginPercent(YGEdge.Horizontal, value.value);
                    break;
            }
        }
    }

    public YGValue MarginVertical
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMargin(YGEdge.Vertical);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMarginAuto(YGEdge.Vertical);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMargin(YGEdge.Vertical, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMarginPercent(YGEdge.Vertical, value.value);
                    break;
            }
        }
    }

    public YGValue Padding
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPadding(YGEdge.All);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPadding(YGEdge.All, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPaddingPercent(YGEdge.All, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPadding(YGEdge.Top);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPadding(YGEdge.Top, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPaddingPercent(YGEdge.Top, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPadding(YGEdge.Left);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPadding(YGEdge.Left, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPaddingPercent(YGEdge.Left, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPadding(YGEdge.Right);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPadding(YGEdge.Right, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPaddingPercent(YGEdge.Right, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPadding(YGEdge.Bottom);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPadding(YGEdge.Bottom, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPaddingPercent(YGEdge.Bottom, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPadding(YGEdge.Horizontal);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPadding(YGEdge.Horizontal, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPaddingPercent(YGEdge.Horizontal, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetPadding(YGEdge.Vertical);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Point:
                    OwnerNode.YGNode->SetPadding(YGEdge.Vertical, value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetPaddingPercent(YGEdge.Vertical, value.value);
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
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetBorder(YGEdge.All);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetBorder(YGEdge.All, value);
        }
    }

    public float BorderTop
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetBorder(YGEdge.Top);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetBorder(YGEdge.Top, value);
        }
    }

    public float BorderLeft
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetBorder(YGEdge.Left);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetBorder(YGEdge.Left, value);
        }
    }

    public float BorderRight
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetBorder(YGEdge.Right);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetBorder(YGEdge.Right, value);
        }
    }

    public float BorderBottom
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetBorder(YGEdge.Bottom);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetBorder(YGEdge.Bottom, value);
        }
    }

    public float BorderHorizontal
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetBorder(YGEdge.Horizontal);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetBorder(YGEdge.Horizontal, value);
        }
    }

    public float BorderVertical
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetBorder(YGEdge.Vertical);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetBorder(YGEdge.Vertical, value);
        }
    }

    public float Gap
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetGap(YGGutter.All);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetGap(YGGutter.All, value);
        }
    }

    public float GapColumn
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetGap(YGGutter.Column);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetGap(YGGutter.Column, value);
        }
    }

    public float GapRow
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetGap(YGGutter.Row);
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetGap(YGGutter.Row, value);
        }
    }

    public float AspectRatio
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetAspectRatio();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();
            OwnerNode.YGNode->SetAspectRatio(value);
        }
    }

    public YGValue Width
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetWidth();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetWidthAuto();
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetWidth(value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetWidthPercent(value.value);
                    break;
            }
        }
    }

    public YGValue Height
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetHeight();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetHeightAuto();
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetHeight(value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetHeightPercent(value.value);
                    break;
            }
        }
    }

    public YGValue MinWidth
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMinWidth();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMinWidth(float.NaN);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMinWidth(value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMinWidthPercent(value.value);
                    break;
            }
        }
    }

    public YGValue MinHeight
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMinHeight();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMinHeight(float.NaN);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMinHeight(value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMinHeightPercent(value.value);
                    break;
            }
        }
    }

    public YGValue MaxWidth
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMaxWidth();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMaxWidth(float.NaN);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMaxWidth(value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMaxWidthPercent(value.value);
                    break;
            }
        }
    }

    public YGValue MaxHeight
    {
        get
        {
            OwnerNode.ThrowIfDisposed();
            return OwnerNode.YGNode->GetMaxHeight();
        }

        set
        {
            OwnerNode.ThrowIfDisposed();

            switch (value.unit)
            {
                case YGUnit.Undefined:
                case YGUnit.Auto:
                    OwnerNode.YGNode->SetMaxHeight(float.NaN);
                    break;

                case YGUnit.Point:
                    OwnerNode.YGNode->SetMaxHeight(value.value);
                    break;

                case YGUnit.Percent:
                    OwnerNode.YGNode->SetMaxHeightPercent(value.value);
                    break;
            }
        }
    }

    #endregion

    #region Borders

    private readonly HaselColor[] _borderColors = new HaselColor[4]; // PhysicalEdge
    private readonly float[] _borderRadius = new float[4]; // Corner

    public HaselColor BorderColor
    {
        get => _borderColors[(byte)PhysicalEdge.Top];
        set
        {
            _borderColors[(byte)PhysicalEdge.Top] = value;
            _borderColors[(byte)PhysicalEdge.Right] = value;
            _borderColors[(byte)PhysicalEdge.Bottom] = value;
            _borderColors[(byte)PhysicalEdge.Left] = value;
        }
    }

    public HaselColor BorderColorTop
    {
        get => _borderColors[(byte)PhysicalEdge.Top];
        set => _borderColors[(byte)PhysicalEdge.Top] = value;
    }

    public HaselColor BorderColorRight
    {
        get => _borderColors[(byte)PhysicalEdge.Right];
        set => _borderColors[(byte)PhysicalEdge.Right] = value;
    }

    public HaselColor BorderColorBottom
    {
        get => _borderColors[(byte)PhysicalEdge.Bottom];
        set => _borderColors[(byte)PhysicalEdge.Bottom] = value;
    }

    public HaselColor BorderColorLeft
    {
        get => _borderColors[(byte)PhysicalEdge.Left];
        set => _borderColors[(byte)PhysicalEdge.Left] = value;
    }

    public HaselColor BorderColorHorizontal
    {
        get => _borderColors[(byte)PhysicalEdge.Right];
        set
        {
            _borderColors[(byte)PhysicalEdge.Right] = value;
            _borderColors[(byte)PhysicalEdge.Left] = value;
        }
    }

    public HaselColor BorderColorVertical
    {
        get => _borderColors[(byte)PhysicalEdge.Top];
        set
        {
            _borderColors[(byte)PhysicalEdge.Top] = value;
            _borderColors[(byte)PhysicalEdge.Bottom] = value;
        }
    }

    public float BorderRadius
    {
        get => _borderRadius[(byte)Corner.TopLeft];
        set
        {
            _borderRadius[(byte)Corner.TopLeft] = value;
            _borderRadius[(byte)Corner.TopRight] = value;
            _borderRadius[(byte)Corner.BottomLeft] = value;
            _borderRadius[(byte)Corner.BottomRight] = value;
        }
    }

    public float BorderRadiusTopLeft
    {
        get => _borderRadius[(byte)Corner.TopLeft];
        set => _borderRadius[(byte)Corner.TopLeft] = value;
    }

    public float BorderRadiusTopRight
    {
        get => _borderRadius[(byte)Corner.TopRight];
        set => _borderRadius[(byte)Corner.TopRight] = value;
    }

    public float BorderRadiusBottomLeft
    {
        get => _borderRadius[(byte)Corner.BottomLeft];
        set => _borderRadius[(byte)Corner.BottomLeft] = value;
    }

    public float BorderRadiusBottomRight
    {
        get => _borderRadius[(byte)Corner.BottomRight];
        set => _borderRadius[(byte)Corner.BottomRight] = value;
    }

    public float BorderRadiusTop
    {
        get => _borderRadius[(byte)Corner.TopLeft];
        set
        {
            _borderRadius[(byte)Corner.TopLeft] = value;
            _borderRadius[(byte)Corner.TopRight] = value;
        }
    }

    public float BorderRadiusBottom
    {
        get => _borderRadius[(byte)Corner.BottomLeft];
        set
        {
            _borderRadius[(byte)Corner.BottomLeft] = value;
            _borderRadius[(byte)Corner.BottomRight] = value;
        }
    }

    #endregion

    internal void ApplyInlineStyle()
    {
        var stylesheet = NodeParser.StylesheetParser.Parse(".s{" + OwnerNode.Attributes["style"] + "}");
        var rule = stylesheet.StyleRules.First();
        Apply(rule.Style);
    }

    internal void Apply(StyleDeclaration declaration)
    {
        foreach (var property in declaration.Declarations)
        {
            ApplyProperty(property.Name, property.Value);
        }
    }

    // TODO: handle property.IsInherited
    // https://stackoverflow.com/a/5612360
    /*
    border-collapse
    border-spacing
    direction
    font-family
    font-size
    font-style
    font-variant
    font-weight
    font
    letter-spacing
    line-height
    list-style-image
    list-style-position
    list-style-type
    list-style
    text-align
    text-indent
    text-transform
    visibility
    white-space
    word-spacing
    */

    // TODO: update sub-nodes style
    private void ApplyProperty(string property, string value)
    {
        switch (property)
        {
            case "align-content":
                AlignContent = value switch
                {
                    "auto" => YGAlign.Auto,
                    "flex-start" => YGAlign.FlexStart,
                    "center" => YGAlign.Center,
                    "flex-end" => YGAlign.FlexEnd,
                    "stretch" => YGAlign.Stretch,
                    "baseline" => YGAlign.Baseline,
                    "space-between" => YGAlign.SpaceBetween,
                    "space-around" => YGAlign.SpaceAround,
                    "space-evenly" => YGAlign.SpaceEvenly,
                    _ => YGAlign.Stretch,
                };
                break;

            case "align-items":
                AlignItems = value switch
                {
                    "auto" => YGAlign.Auto,
                    "flex-start" => YGAlign.FlexStart,
                    "center" => YGAlign.Center,
                    "flex-end" => YGAlign.FlexEnd,
                    "stretch" => YGAlign.Stretch,
                    "baseline" => YGAlign.Baseline,
                    "space-between" => YGAlign.SpaceBetween,
                    "space-around" => YGAlign.SpaceAround,
                    "space-evenly" => YGAlign.SpaceEvenly,
                    _ => YGAlign.Stretch,
                };
                break;

            case "align-self":
                AlignSelf = value switch
                {
                    "auto" => YGAlign.Auto,
                    "flex-start" => YGAlign.FlexStart,
                    "center" => YGAlign.Center,
                    "flex-end" => YGAlign.FlexEnd,
                    "stretch" => YGAlign.Stretch,
                    "baseline" => YGAlign.Baseline,
                    "space-between" => YGAlign.SpaceBetween,
                    "space-around" => YGAlign.SpaceAround,
                    "space-evenly" => YGAlign.SpaceEvenly,
                    _ => YGAlign.Stretch,
                };
                break;

            case "direction":
                Direction = value switch
                {
                    "inherit" => YGDirection.Inherit,
                    "ltr" => YGDirection.LTR,
                    "rtl" => YGDirection.RTL,
                    _ => YGDirection.Inherit,
                };
                break;

            case "display":
                Display = value switch
                {
                    "flex" => YGDisplay.Flex,
                    "none" => YGDisplay.None,
                    _ => YGDisplay.Flex,
                };
                break;

            case "flex-direction":
                FlexDirection = value switch
                {
                    "column" => YGFlexDirection.Column,
                    "column-reverse" => YGFlexDirection.ColumnReverse,
                    "row" => YGFlexDirection.Row,
                    "row-reverse" => YGFlexDirection.RowReverse,
                    _ => YGFlexDirection.Row,
                };
                break;

            case "justify-content":
                JustifyContent = value switch
                {
                    "flex-start" => YGJustify.FlexStart,
                    "center" => YGJustify.Center,
                    "flex-end" => YGJustify.FlexEnd,
                    "space-between" => YGJustify.SpaceBetween,
                    "space-around" => YGJustify.SpaceAround,
                    "space-evenly" => YGJustify.SpaceEvenly,
                    _ => YGJustify.FlexStart,
                };
                break;

            case "overflow":
                Overflow = value switch
                {
                    "visible" => YGOverflow.Visible,
                    "hidden" => YGOverflow.Hidden,
                    "scroll" => YGOverflow.Scroll,
                    _ => YGOverflow.Visible,
                };
                break;

            case "position":
                PositionType = value switch
                {
                    "static" => YGPositionType.Static,
                    "relative" => YGPositionType.Relative,
                    "absolute" => YGPositionType.Absolute,
                    _ => YGPositionType.Relative,
                };
                break;

            case "flex-wrap":
                FlexWrap = value switch
                {
                    "no-wrap" => YGWrap.NoWrap,
                    "wrap" => YGWrap.Wrap,
                    "wrap-reverse" => YGWrap.WrapReverse,
                    _ => YGWrap.Wrap,
                };
                break;

            case "flex":
                Flex = NodeParser.ParseFloat(value); // TODO: not handled like in css
                break;

            case "flex-grow":
                FlexGrow = NodeParser.ParseFloat(value);
                break;

            case "flex-shrink":
                FlexShrink = NodeParser.ParseFloat(value);
                break;

            case "flex-basis":
                FlexBasis = NodeParser.ParseYGValue(value);
                break;

            case "top":
                PositionTop = NodeParser.ParseYGValue(value);
                break;

            case "left":
                PositionLeft = NodeParser.ParseYGValue(value);
                break;

            case "aspect-ratio":
                AspectRatio = NodeParser.ParseFloat(value);
                break;

            case "width":
                Width = NodeParser.ParseYGValue(value);
                break;

            case "height":
                Height = NodeParser.ParseYGValue(value);
                break;

            case "min-width":
                MinWidth = NodeParser.ParseYGValue(value);
                break;

            case "min-height":
                MinHeight = NodeParser.ParseYGValue(value);
                break;

            case "max-width":
                MaxWidth = NodeParser.ParseYGValue(value);
                break;

            case "max-height":
                MaxHeight = NodeParser.ParseYGValue(value);
                break;

            case "margin":
                Margin = NodeParser.ParseYGValue(value); // TODO: not handled like in css
                break;

            case "margin-top":
                MarginTop = NodeParser.ParseYGValue(value);
                break;

            case "margin-right":
                MarginRight = NodeParser.ParseYGValue(value);
                break;

            case "margin-bottom":
                MarginBottom = NodeParser.ParseYGValue(value);
                break;

            case "margin-left":
                MarginLeft = NodeParser.ParseYGValue(value);
                break;

            case "padding":
                Padding = NodeParser.ParseYGValue(value); // TODO: not handled like in css
                break;

            case "padding-top":
                PaddingTop = NodeParser.ParseYGValue(value);
                break;

            case "padding-right":
                PaddingRight = NodeParser.ParseYGValue(value);
                break;

            case "padding-bottom":
                PaddingBottom = NodeParser.ParseYGValue(value);
                break;

            case "padding-left":
                PaddingLeft = NodeParser.ParseYGValue(value);
                break;

            case "border":
                {
                    var result = NodeParser.ParseStyleBorder(value);

                    switch (result.LineWidth.Length)
                    {
                        case 1:
                            Border = result.LineWidth[0];
                            break;
                        case 2:
                            BorderVertical = result.LineWidth[0];
                            BorderHorizontal = result.LineWidth[1];
                            break;
                        case 3:
                            BorderTop = result.LineWidth[0];
                            BorderRight = result.LineWidth[1];
                            BorderBottom = result.LineWidth[2];
                            BorderLeft = result.LineWidth[1];
                            break;
                        case 4:
                            BorderTop = result.LineWidth[0];
                            BorderRight = result.LineWidth[1];
                            BorderBottom = result.LineWidth[2];
                            BorderLeft = result.LineWidth[3];
                            break;
                    }

                    if (result.Color.HasValue)
                        BorderColor = result.Color.Value;
                }
                break;

            case "border-top-width":
                BorderTop = NodeParser.ParseFloat(value);
                break;

            case "border-right-width":
                BorderRight = NodeParser.ParseFloat(value);
                break;

            case "border-bottom-width":
                BorderBottom = NodeParser.ParseFloat(value);
                break;

            case "border-left-width":
                BorderLeft = NodeParser.ParseFloat(value);
                break;

            case "border-color":
                BorderColor = NodeParser.ParseStyleColor(value);
                break;

            case "border-top-color":
                BorderColorTop = NodeParser.ParseStyleColor(value);
                break;

            case "border-right-color":
                BorderColorRight = NodeParser.ParseStyleColor(value);
                break;

            case "border-bottom-color":
                BorderColorBottom = NodeParser.ParseStyleColor(value);
                break;

            case "border-left-color":
                BorderColorLeft = NodeParser.ParseStyleColor(value);
                break;

            case "border-top-left-radius":
                value = !value.Contains(' ') ? value : value[value.IndexOf(' ')..];
                BorderRadiusTopLeft = NodeParser.ParseFloat(value);
                break;

            case "border-top-right-radius":
                value = !value.Contains(' ') ? value : value[value.IndexOf(' ')..];
                BorderRadiusTopRight = NodeParser.ParseFloat(value);
                break;

            case "border-bottom-left-radius":
                value = !value.Contains(' ') ? value : value[value.IndexOf(' ')..];
                BorderRadiusBottomLeft = NodeParser.ParseFloat(value);
                break;

            case "border-bottom-right-radius":
                value = !value.Contains(' ') ? value : value[value.IndexOf(' ')..];
                BorderRadiusBottomRight = NodeParser.ParseFloat(value);
                break;

            case "gap":
                Gap = NodeParser.ParseFloat(value); // TODO: not handled like in css
                break;

            case "column-gap":
                GapColumn = NodeParser.ParseFloat(value);
                break;

            case "row-gap":
                GapRow = NodeParser.ParseFloat(value);
                break;

            case "background-color":
                BackgroundColor = NodeParser.ParseStyleColor(value);
                break;

            case "cursor":
                if (value == "inherit")
                {
                    Cursor.SetInherited();
                    break;
                }

                Cursor.SetValue(value switch
                {
                    "hand" => Enums.Cursor.Hand,
                    "not-allowed" => Enums.Cursor.NotAllowed,
                    "text" => Enums.Cursor.Text,
                    "ns-resize" => Enums.Cursor.ResizeNS,
                    "ew-resize" => Enums.Cursor.ResizeEW,
                    _ => Enums.Cursor.Pointer
                });
                break;

            case "color":
                if (value == "inherit")
                    Color.SetInherited();
                else
                    Color.SetValue(NodeParser.ParseStyleColor(value));
                break;

            case "font-name":
                if (value == "inherit")
                    FontName.SetInherited();
                else
                    FontName.SetValue(value);
                break;

            case "font-size":
                if (value == "inherit")
                    FontSize.SetInherited();
                else
                    FontSize.SetValue(NodeParser.ParseFloat(value));
                break;

            default:
                OwnerNode.GetDocument()?.Logger?.LogError("Unsupported style property \"{property}\" with value \"{value}\"", property, value);
                break;
        }
    }
}
