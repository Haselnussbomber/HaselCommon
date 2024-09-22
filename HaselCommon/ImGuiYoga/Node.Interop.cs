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
