using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using HaselCommon.Structs;
using HaselCommon.Yoga;
using ImGuiNET;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.GroupPoseModule;
using YGMeasureFuncDelegate = HaselCommon.Yoga.Interop.YGMeasureFuncDelegate;

namespace HaselCommon.Utils.ImGuiYoga;

public unsafe class Node : IDisposable
{
    private readonly Dictionary<string, object> Data = [];
    private readonly List<Node> Children = [];
    private readonly HaselColor[] BorderColors = new HaselColor[4]; // PhysicalEdge
    private readonly float[] BorderRadius = new float[4]; // TopLeft, TopRight, BottomLeft, BottomRight

    private YGNode* node;
    private GCHandle? measureFuncHandle;
    private Node? parent;

    public YogaWindow Window { get; }
    public string? Id { get; set; } = null;
    public float ScrollTop { get; set; }
    public float ScrollLeft { get; set; }
    public float ScrollHeight { get; set; }
    public float ScrollWidth { get; set; }

    public Node(YogaWindow window)
    {
        Window = window;
        node = YGNode.New();
    }

    ~Node()
    {
        Dispose();
    }

    public void DisposeRecursive()
    {
        if (Children.Any())
        {
            foreach (var child in Children.ToArray())
            {
                RemoveChild(child);
                child.DisposeRecursive();
            }

            Children.Clear();
        }

        Dispose();
    }

    public void Dispose()
    {
        measureFuncHandle?.Free();
        measureFuncHandle = null;

        if (node != null)
        {
            node->Dispose();
            node = null;
        }
    }

    public void SetData<T>(string key, T value) where T : struct
    {
        Data.Add(key, value);
    }

    public T? GetData<T>(string key) where T : struct
    {
        return Data.TryGetValue(key, out var value) ? (T)value : null;
    }

    public Vector2 GetAbsolutePosition()
    {
        var node = this;
        var pos = GetComputedPosition();

        while ((node = node.GetParent()) != null)
            pos += node.GetComputedPosition();

        return pos;
    }

    public float GetAbsoluteScrollTop()
    {
        var node = this;
        var pos = ScrollTop;

        while ((node = node.GetParent()) != null)
            pos += node.ScrollTop;

        return pos;
    }

    public void CheckDisposed()
    {
        if (node == null)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    public void AppendChild(Node child)
    {
        CheckDisposed();
        Children.Add(child);
        child.SetParent(this);
        node->AppendChild(child);
    }

    public void InsertChild(Node child, int index)
    {
        CheckDisposed();
        Children.Insert(index, child);
        child.SetParent(this);
        node->InsertChild(child, index);
    }

    public void SwapChild(Node child, int index)
    {
        CheckDisposed();
        Children[index] = child;
        child.SetParent(this);
        node->SwapChild(child, index);
    }

    public void RemoveChild(Node excludedChild)
    {
        CheckDisposed();
        Children.Remove(excludedChild);
        excludedChild.SetParent(null);
        node->RemoveChild(excludedChild);
    }

    public void SetParent(Node? parent)
    {
        this.parent = parent;
    }

    public Node? GetParent()
    {
        return parent;
    }

    public Node GetChild(int index)
    {
        return Children[index];
    }

    public int GetChildCount()
    {
        return Children.Count;
    }

    public void SetBorderColor(Edge edge, HaselColor color)
    {
        switch (edge)
        {
            case Edge.Horizontal:
                BorderColors[(byte)Edge.Left] = color;
                BorderColors[(byte)Edge.Right] = color;
                break;

            case Edge.Vertical:
                BorderColors[(byte)Edge.Top] = color;
                BorderColors[(byte)Edge.Bottom] = color;
                break;

            case Edge.All:
                BorderColors[(byte)Edge.Top] = color;
                BorderColors[(byte)Edge.Right] = color;
                BorderColors[(byte)Edge.Bottom] = color;
                BorderColors[(byte)Edge.Left] = color;
                break;

            case Edge.Start:
                BorderColors[(byte)Edge.Left] = color;
                BorderColors[(byte)Edge.Top] = color;
                break;

            case Edge.End:
                BorderColors[(byte)Edge.Right] = color;
                BorderColors[(byte)Edge.Bottom] = color;
                break;

            case Edge.Left:
            case Edge.Right:
            case Edge.Top:
            case Edge.Bottom:
                BorderColors[(byte)edge] = color;
                break;

        }
    }

    public HaselColor GetBorderColor(PhysicalEdge edge)
    {
        return BorderColors[(byte)edge];
    }

    public void SetBorderRadius(Corner corner, float radius)
    {
        BorderRadius[(byte)corner] = radius;
    }

    public float GetBorderRadius(Corner corner)
    {
        return BorderRadius[(byte)corner];
    }

    public virtual void Draw()
    {
        PreDraw();

        PostDraw();
    }

    public virtual void PreDraw()
    {
        var absolutePos = Window.ViewportPosition + GetComputedPosition();

        var width = node->GetComputedWidth();
        var height = node->GetComputedHeight();

        var borderTop = node->GetComputedBorder(Edge.Top);
        var borderRight = node->GetComputedBorder(Edge.Right);
        var borderBottom = node->GetComputedBorder(Edge.Bottom);
        var borderLeft = node->GetComputedBorder(Edge.Left);

        var borderRadiusTopLeft = GetBorderRadius(Corner.TopLeft);
        var borderRadiusTopRight = GetBorderRadius(Corner.TopRight);
        var borderRadiusBottomLeft = GetBorderRadius(Corner.BottomLeft);
        var borderRadiusBottomRight = GetBorderRadius(Corner.BottomRight);

        // TODO: rounded borders don't have a color gradient
        // TODO: https://drafts.csswg.org/css-backgrounds-3/#corner-shaping

        if ((borderTop > 0 || borderLeft > 0) && borderRadiusTopLeft > 0)
        {
            ImGui.GetWindowDrawList().AddBezierQuadratic(
                absolutePos + new Vector2(0, borderRadiusTopLeft),
                absolutePos,
                absolutePos + new Vector2(borderRadiusTopLeft, 0),
                GetBorderColor(PhysicalEdge.Top),
                borderTop);
        }

        if ((borderTop > 0 || borderRight > 0) && borderRadiusTopRight > 0)
        {
            ImGui.GetWindowDrawList().AddBezierQuadratic(
                absolutePos + new Vector2(width - borderRadiusTopRight, 0),
                absolutePos + new Vector2(width, 0),
                absolutePos + new Vector2(width, borderRadiusTopRight),
                GetBorderColor(PhysicalEdge.Top),
                borderTop);
        }

        if ((borderBottom > 0 || borderRight > 0) && borderRadiusBottomRight > 0)
        {
            ImGui.GetWindowDrawList().AddBezierQuadratic(
                absolutePos + new Vector2(width - borderRadiusBottomRight, height),
                absolutePos + new Vector2(width, height),
                absolutePos + new Vector2(width, height - borderRadiusBottomRight),
                GetBorderColor(PhysicalEdge.Bottom),
                borderBottom);
        }

        if ((borderBottom > 0 || borderLeft > 0) && borderRadiusBottomLeft > 0)
        {
            ImGui.GetWindowDrawList().AddBezierQuadratic(
                absolutePos + new Vector2(width - borderRadiusBottomRight, height),
                absolutePos + new Vector2(width, height),
                absolutePos + new Vector2(width, height - borderRadiusBottomRight),
                GetBorderColor(PhysicalEdge.Bottom),
                borderBottom);
        }

        if (borderTop > 0)
        {
            ImGui.GetWindowDrawList().AddLine(
                absolutePos + new Vector2(borderRadiusTopLeft, 0),
                absolutePos + new Vector2(width - borderRadiusTopRight, 0),
                GetBorderColor(PhysicalEdge.Top),
                borderTop);
        }

        if (borderRight > 0)
        {
            ImGui.GetWindowDrawList().AddLine(
                absolutePos + new Vector2(width - borderRight, borderRadiusTopRight),
                absolutePos + new Vector2(width - borderRight, height - borderRadiusBottomRight),
                GetBorderColor(PhysicalEdge.Right),
                borderRight);
        }

        if (borderBottom > 0)
        {
            ImGui.GetWindowDrawList().AddLine(
                absolutePos + new Vector2(0 + borderRadiusBottomLeft, height - borderBottom),
                absolutePos + new Vector2(width - borderRadiusBottomRight, height - borderBottom),
                GetBorderColor(PhysicalEdge.Bottom),
                borderBottom);
        }

        if (borderLeft > 0)
        {
            ImGui.GetWindowDrawList().AddLine(
                absolutePos + new Vector2(0, borderRadiusTopLeft),
                absolutePos + new Vector2(0, height - borderRadiusBottomLeft),
                GetBorderColor(PhysicalEdge.Left),
                borderLeft);
        }
    }

    public virtual void PostDraw()
    {
        if (!Children.Any())
            return;

        var pos = GetAbsolutePosition();
        var maxPos = GetAbsolutePosition() + GetComputedSize();

        ImGui.PushClipRect(Window.ViewportPosition + pos, Window.ViewportPosition + maxPos, false);

        foreach (var child in Children)
        {
            var childPos = child.GetAbsolutePosition() - new Vector2(ScrollLeft, ScrollTop);
            var childMaxPos = childPos + child.GetComputedSize();
            if (childMaxPos.X < pos.X || childMaxPos.Y < pos.Y || childPos.X > maxPos.X || childPos.Y > maxPos.Y)
                continue;

            child.Draw();
        }

        ImGui.PopClipRect();

        if (GetHadOverflow() /* && mouse inside rect */) // also needs check if lower level nodes are still scrollable
        {
            CalculateLayout();

            const int WHEEL_DELTA = 120;

            var height = GetComputedHeight();
            var parentHeight = parent?.GetComputedHeight() ?? height;

            ImGui.TextUnformatted($"parentHeight: {parentHeight}");

            var wheelState = ImGui.GetIO().MouseWheel;
            if (wheelState < 0)
            {
                var newScrollTop = Math.Clamp(ScrollTop + WHEEL_DELTA, 0, height - parentHeight);

                ScrollTop = newScrollTop;
            }
            else if (wheelState > 0)
            {
                ScrollTop = Math.Max(0, ScrollTop - WHEEL_DELTA);
            }

            ImGui.GetWindowDrawList().AddRectFilled(
                Window.ViewportPosition + pos + new Vector2(GetComputedWidth() - 10),
                Window.ViewportPosition + pos + new Vector2(GetComputedWidth(), parentHeight),
                ImGui.GetColorU32(ImGuiCol.ScrollbarGrab),
                3);
        }
    }

    #region YGNode API

    /// <inheritdoc cref="YGNode.Reset()" />
    public void Reset()
    {
        CheckDisposed();
        node->Reset();
    }

    /// <inheritdoc cref="YGNode.CalculateLayout(float, float, Direction)" />
    public void CalculateLayout(float ownerWidth = float.NaN, float ownerHeight = float.NaN, Direction ownerDirection = Direction.LTR)
    {
        CheckDisposed();
        node->CalculateLayout(ownerWidth, ownerHeight, ownerDirection);
    }

    /// <inheritdoc cref="YGNode.GetHasNewLayout()" />
    public bool GetHasNewLayout()
    {
        CheckDisposed();
        return node->GetHasNewLayout();
    }

    /// <inheritdoc cref="YGNode.SetHasNewLayout(bool)" />
    public void SetHasNewLayout(bool hasNewLayout)
    {
        CheckDisposed();
        node->SetHasNewLayout(hasNewLayout);
    }

    /// <inheritdoc cref="YGNode.IsDirty()" />
    public bool IsDirty()
    {
        CheckDisposed();
        return node->IsDirty();
    }

    /// <inheritdoc cref="YGNode.MarkDirty()" />
    public void MarkDirty()
    {
        CheckDisposed();
        node->MarkDirty();
    }

    /* handled via ImGuiNode

    /// <inheritdoc cref="YGNode.InsertChild(YGNode*, int)" />
    public void InsertChild(YGNode* child, int index)
    {
        CheckDisposed();
        Node->InsertChild(child, index);
    }

    /// <inheritdoc cref="YGNode.SwapChild(YGNode*, int)" />
    public void SwapChild(YGNode* child, int index)
    {
        CheckDisposed();
        Node->SwapChild(child, index);
    }

    /// <inheritdoc cref="YGNode.RemoveChild(YGNode*)" />
    public void RemoveChild(YGNode* excludedChild)
    {
        CheckDisposed();
        Node->RemoveChild(excludedChild);
    }

    /// <inheritdoc cref="YGNode.RemoveAllChildren()" />
    public void RemoveAllChildren()
    {
        CheckDisposed();
        Children.Clear();
        Node->RemoveAllChildren();
    }

    /// <inheritdoc cref="YGNode.SetChildren(YGNode*[])" />
    public unsafe void SetChildren(params YGNode*[] children)
    {
        CheckDisposed();
        Node->SetChildren(children);
    }

    /// <inheritdoc cref="YGNode.GetChild(int)" />
    public YGNode* GetChild(int index)
    {
        CheckDisposed();
        return Node->GetChild(index);
    }

    /// <inheritdoc cref="YGNode.GetChildCount()" />
    public int GetChildCount()
    {
        CheckDisposed();
        return Node->GetChildCount();
    }
    */

    /// <inheritdoc cref="YGNode.GetOwner()" />
    public YGNode* GetOwner()
    {
        CheckDisposed();
        return node->GetOwner();
    }

    /// <inheritdoc cref="YGNode.SetConfig(YGConfig*)" />
    public void SetConfig(YGConfig* config)
    {
        CheckDisposed();
        node->SetConfig(config);
    }

    /// <inheritdoc cref="YGNode.GetConfig()" />
    public YGConfig* GetConfig()
    {
        CheckDisposed();
        return node->GetConfig();
    }

    /// <inheritdoc cref="YGNode.SetContext(void*)" />
    public void SetContext(void* context)
    {
        CheckDisposed();
        node->SetContext(context);
    }

    /// <inheritdoc cref="YGNode.GetContext()" />
    public void* GetContext()
    {
        CheckDisposed();
        return node->GetContext();
    }

    /// <inheritdoc cref="YGNode.SetMeasureFunc(nint)" />
    public void SetMeasureFunc(YGMeasureFuncDelegate measureFuncDelege)
    {
        measureFuncHandle = GCHandle.Alloc(measureFuncDelege);
        node->SetMeasureFunc(Marshal.GetFunctionPointerForDelegate(measureFuncDelege));
    }

    /// <inheritdoc cref="YGNode.HasMeasureFunc()" />
    public bool HasMeasureFunc()
    {
        CheckDisposed();
        return node->HasMeasureFunc();
    }

    /// <inheritdoc cref="YGNode.SetBaselineFunc(nint)" />
    public void SetBaselineFunc(nint baselineFunc)
    {
        CheckDisposed();
        node->SetBaselineFunc(baselineFunc);
    }

    /// <inheritdoc cref="YGNode.HasBaselineFunc()" />
    public bool HasBaselineFunc()
    {
        CheckDisposed();
        return node->HasBaselineFunc();
    }

    /// <inheritdoc cref="YGNode.SetIsReferenceBaseline(bool)" />
    public void SetIsReferenceBaseline(bool isReferenceBaseline)
    {
        CheckDisposed();
        node->SetIsReferenceBaseline(isReferenceBaseline);
    }

    /// <inheritdoc cref="YGNode.IsReferenceBaseline()" />
    public bool IsReferenceBaseline()
    {
        CheckDisposed();
        return node->IsReferenceBaseline();
    }

    /// <inheritdoc cref="YGNode.SetNodeType(NodeType)" />
    public void SetNodeType(NodeType nodeType)
    {
        CheckDisposed();
        node->SetNodeType(nodeType);
    }

    /// <inheritdoc cref="YGNode.GetNodeType()" />
    public NodeType GetNodeType()
    {
        CheckDisposed();
        return node->GetNodeType();
    }

    /// <inheritdoc cref="YGNode.SetAlwaysFormsContainingBlock(bool)" />
    public void SetAlwaysFormsContainingBlock(bool alwaysFormsContainingBlock)
    {
        CheckDisposed();
        node->SetAlwaysFormsContainingBlock(alwaysFormsContainingBlock);
    }

    /// <inheritdoc cref="YGNode.GetAlwaysFormsContainingBlock()" />
    public bool GetAlwaysFormsContainingBlock()
    {
        CheckDisposed();
        return node->GetAlwaysFormsContainingBlock();
    }

    /// <inheritdoc cref="YGNode.GetComputedLeft()" />
    public float GetComputedLeft()
    {
        CheckDisposed();
        return node->GetComputedLeft();
    }

    /// <inheritdoc cref="YGNode.GetComputedTop()" />
    public float GetComputedTop()
    {
        CheckDisposed();
        return node->GetComputedTop();
    }

    /// <inheritdoc cref="YGNode.GetComputedRight()" />
    public float GetComputedRight()
    {
        CheckDisposed();
        return node->GetComputedRight();
    }

    /// <inheritdoc cref="YGNode.GetComputedBottom()" />
    public float GetComputedBottom()
    {
        CheckDisposed();
        return node->GetComputedBottom();
    }

    /// <inheritdoc cref="YGNode.GetComputedWidth()" />
    public float GetComputedWidth()
    {
        CheckDisposed();
        return node->GetComputedWidth();
    }

    /// <inheritdoc cref="YGNode.GetComputedHeight()" />
    public float GetComputedHeight()
    {
        CheckDisposed();
        return node->GetComputedHeight();
    }

    /// <inheritdoc cref="YGNode.GetComputedDirection()" />
    public Direction GetComputedDirection()
    {
        CheckDisposed();
        return node->GetComputedDirection();
    }

    /// <inheritdoc cref="YGNode.GetHadOverflow()" />
    public bool GetHadOverflow()
    {
        CheckDisposed();
        return node->GetHadOverflow();
    }

    /// <inheritdoc cref="YGNode.GetComputedMargin(Edge)" />
    public float GetComputedMargin(Edge edge)
    {
        CheckDisposed();
        return node->GetComputedMargin(edge);
    }

    /// <inheritdoc cref="YGNode.GetComputedBorder(Edge)" />
    public float GetComputedBorder(Edge edge)
    {
        CheckDisposed();
        return node->GetComputedBorder(edge);
    }

    /// <inheritdoc cref="YGNode.GetComputedPadding(Edge)" />
    public float GetComputedPadding(Edge edge)
    {
        CheckDisposed();
        return node->GetComputedPadding(edge);
    }

    /// <inheritdoc cref="YGNode.CopyStyle(YGNode*)" />
    public void CopyStyle(YGNode* srcNode)
    {
        CheckDisposed();
        node->CopyStyle(srcNode);
    }

    /// <inheritdoc cref="YGNode.SetDirection(Direction)" />
    public void SetDirection(Direction value)
    {
        CheckDisposed();
        node->SetDirection(value);
    }

    /// <inheritdoc cref="YGNode.GetDirection()" />
    public Direction GetDirection()
    {
        CheckDisposed();
        return node->GetDirection();
    }

    /// <inheritdoc cref="YGNode.SetFlexDirection(FlexDirection)" />
    public void SetFlexDirection(FlexDirection flexDirection)
    {
        CheckDisposed();
        node->SetFlexDirection(flexDirection);
    }

    /// <inheritdoc cref="YGNode.GetFlexDirection()" />
    public FlexDirection GetFlexDirection()
    {
        CheckDisposed();
        return node->GetFlexDirection();
    }

    /// <inheritdoc cref="YGNode.SetJustifyContent(Justify)" />
    public void SetJustifyContent(Justify justifyContent)
    {
        CheckDisposed();
        node->SetJustifyContent(justifyContent);
    }

    /// <inheritdoc cref="YGNode.GetJustifyContent()" />
    public Justify GetJustifyContent()
    {
        CheckDisposed();
        return node->GetJustifyContent();
    }

    /// <inheritdoc cref="YGNode.SetAlignContent(Align)" />
    public void SetAlignContent(Align alignContent)
    {
        CheckDisposed();
        node->SetAlignContent(alignContent);
    }

    /// <inheritdoc cref="YGNode.GetAlignContent()" />
    public Align GetAlignContent()
    {
        CheckDisposed();
        return node->GetAlignContent();
    }

    /// <inheritdoc cref="YGNode.SetAlignItems(Align)" />
    public void SetAlignItems(Align alignItems)
    {
        CheckDisposed();
        node->SetAlignItems(alignItems);
    }

    /// <inheritdoc cref="YGNode.GetAlignItems()" />
    public Align GetAlignItems()
    {
        CheckDisposed();
        return node->GetAlignItems();
    }

    /// <inheritdoc cref="YGNode.SetAlignSelf(Align)" />
    public void SetAlignSelf(Align alignSelf)
    {
        CheckDisposed();
        node->SetAlignSelf(alignSelf);
    }

    /// <inheritdoc cref="YGNode.GetAlignSelf()" />
    public Align GetAlignSelf()
    {
        CheckDisposed();
        return node->GetAlignSelf();
    }

    /// <inheritdoc cref="YGNode.SetPositionType(PositionType)" />
    public void SetPositionType(PositionType positionType)
    {
        CheckDisposed();
        node->SetPositionType(positionType);
    }

    /// <inheritdoc cref="YGNode.GetPositionType()" />
    public PositionType GetPositionType()
    {
        CheckDisposed();
        return node->GetPositionType();
    }

    /// <inheritdoc cref="YGNode.SetFlexWrap(Wrap)" />
    public void SetFlexWrap(Wrap flexWrap)
    {
        CheckDisposed();
        node->SetFlexWrap(flexWrap);
    }

    /// <inheritdoc cref="YGNode.GetFlexWrap()" />
    public Wrap GetFlexWrap()
    {
        CheckDisposed();
        return node->GetFlexWrap();
    }

    /// <inheritdoc cref="YGNode.SetOverflow(Overflow)" />
    public void SetOverflow(Overflow overflow)
    {
        CheckDisposed();
        node->SetOverflow(overflow);
    }

    /// <inheritdoc cref="YGNode.GetOverflow()" />
    public Overflow GetOverflow()
    {
        CheckDisposed();
        return node->GetOverflow();
    }

    /// <inheritdoc cref="YGNode.SetDisplay(Display)" />
    public void SetDisplay(Display display)
    {
        CheckDisposed();
        node->SetDisplay(display);
    }

    /// <inheritdoc cref="YGNode.GetDisplay()" />
    public Display GetDisplay()
    {
        CheckDisposed();
        return node->GetDisplay();
    }

    /// <inheritdoc cref="YGNode.SetFlex(float)" />
    public void SetFlex(float flex)
    {
        CheckDisposed();
        node->SetFlex(flex);
    }

    /// <inheritdoc cref="YGNode.GetFlex()" />
    public float GetFlex()
    {
        CheckDisposed();
        return node->GetFlex();
    }

    /// <inheritdoc cref="YGNode.SetFlexGrow(float)" />
    public void SetFlexGrow(float flexGrow)
    {
        CheckDisposed();
        node->SetFlexGrow(flexGrow);
    }

    /// <inheritdoc cref="YGNode.GetFlexGrow()" />
    public float GetFlexGrow()
    {
        CheckDisposed();
        return node->GetFlexGrow();
    }

    /// <inheritdoc cref="YGNode.SetFlexShrink(float)" />
    public void SetFlexShrink(float flexShrink)
    {
        CheckDisposed();
        node->SetFlexShrink(flexShrink);
    }

    /// <inheritdoc cref="YGNode.GetFlexShrink()" />
    public float GetFlexShrink()
    {
        CheckDisposed();
        return node->GetFlexShrink();
    }

    /// <inheritdoc cref="YGNode.SetFlexBasis(float)" />
    public void SetFlexBasis(float flexBasis)
    {
        CheckDisposed();
        node->SetFlexBasis(flexBasis);
    }

    /// <inheritdoc cref="YGNode.SetFlexBasisPercent(float)" />
    public void SetFlexBasisPercent(float flexBasisPercent)
    {
        CheckDisposed();
        node->SetFlexBasisPercent(flexBasisPercent);
    }

    /// <inheritdoc cref="YGNode.SetFlexBasisAuto()" />
    public void SetFlexBasisAuto()
    {
        CheckDisposed();
        node->SetFlexBasisAuto();
    }

    /// <inheritdoc cref="YGNode.GetFlexBasis()" />
    public YGValue GetFlexBasis()
    {
        CheckDisposed();
        return node->GetFlexBasis();
    }

    /// <inheritdoc cref="YGNode.SetPosition(Edge, float)" />
    public void SetPosition(Edge edge, float points)
    {
        CheckDisposed();
        node->SetPosition(edge, points);
    }

    /// <inheritdoc cref="YGNode.SetPositionPercent(Edge, float)" />
    public void SetPositionPercent(Edge edge, float percent)
    {
        CheckDisposed();
        node->SetPositionPercent(edge, percent);
    }

    /// <inheritdoc cref="YGNode.GetPosition(Edge)" />
    public YGValue GetPosition(Edge edge)
    {
        CheckDisposed();
        return node->GetPosition(edge);
    }

    /// <inheritdoc cref="YGNode.SetMargin(Edge, float)" />
    public void SetMargin(Edge edge, float points)
    {
        CheckDisposed();
        node->SetMargin(edge, points);
    }

    /// <inheritdoc cref="YGNode.SetMarginPercent(Edge, float)" />
    public void SetMarginPercent(Edge edge, float percent)
    {
        CheckDisposed();
        node->SetMarginPercent(edge, percent);
    }

    /// <inheritdoc cref="YGNode.SetMarginAuto(Edge)" />
    public void SetMarginAuto(Edge edge)
    {
        CheckDisposed();
        node->SetMarginAuto(edge);
    }

    /// <inheritdoc cref="YGNode.GetMargin(Edge)" />
    public YGValue GetMargin(Edge edge)
    {
        CheckDisposed();
        return node->GetMargin(edge);
    }

    /// <inheritdoc cref="YGNode.SetPadding(Edge, float)" />
    public void SetPadding(Edge edge, float points)
    {
        CheckDisposed();
        node->SetPadding(edge, points);
    }

    /// <inheritdoc cref="YGNode.SetPaddingPercent(Edge, float)" />
    public void SetPaddingPercent(Edge edge, float percent)
    {
        CheckDisposed();
        node->SetPaddingPercent(edge, percent);
    }

    /// <inheritdoc cref="YGNode.GetPadding(Edge)" />
    public YGValue GetPadding(Edge edge)
    {
        CheckDisposed();
        return node->GetPadding(edge);
    }

    /// <inheritdoc cref="YGNode.SetBorder(Edge, float)" />
    public void SetBorder(Edge edge, float border)
    {
        CheckDisposed();
        node->SetBorder(edge, border);
    }

    /// <inheritdoc cref="YGNode.GetBorder(Edge)" />
    public float GetBorder(Edge edge)
    {
        CheckDisposed();
        return node->GetBorder(edge);
    }

    /// <inheritdoc cref="YGNode.SetGap(Gutter, float)" />
    public void SetGap(Gutter gutter, float gapLength)
    {
        CheckDisposed();
        node->SetGap(gutter, gapLength);
    }

    /// <inheritdoc cref="YGNode.GetGap(Gutter)" />
    public float GetGap(Gutter gutter)
    {
        CheckDisposed();
        return node->GetGap(gutter);
    }

    /// <inheritdoc cref="YGNode.SetAspectRatio(float)" />
    public void SetAspectRatio(float aspectRatio)
    {
        CheckDisposed();
        node->SetAspectRatio(aspectRatio);
    }

    /// <inheritdoc cref="YGNode.GetAspectRatio()" />
    public float GetAspectRatio()
    {
        CheckDisposed();
        return node->GetAspectRatio();
    }

    /// <inheritdoc cref="YGNode.SetWidth(float)" />
    public void SetWidth(float points)
    {
        CheckDisposed();
        node->SetWidth(points);
    }

    /// <inheritdoc cref="YGNode.SetWidthPercent(float)" />
    public void SetWidthPercent(float percent)
    {
        CheckDisposed();
        node->SetWidthPercent(percent);
    }

    /// <inheritdoc cref="YGNode.SetWidthAuto()" />
    public void SetWidthAuto()
    {
        CheckDisposed();
        node->SetWidthAuto();
    }

    /// <inheritdoc cref="YGNode.GetWidth()" />
    public YGValue GetWidth()
    {
        CheckDisposed();
        return node->GetWidth();
    }

    /// <inheritdoc cref="YGNode.SetHeight(float)" />
    public void SetHeight(float points)
    {
        CheckDisposed();
        node->SetHeight(points);
    }

    /// <inheritdoc cref="YGNode.SetHeightPercent(float)" />
    public void SetHeightPercent(float percent)
    {
        CheckDisposed();
        node->SetHeightPercent(percent);
    }

    /// <inheritdoc cref="YGNode.SetHeightAuto()" />
    public void SetHeightAuto()
    {
        CheckDisposed();
        node->SetHeightAuto();
    }

    /// <inheritdoc cref="YGNode.GetHeight()" />
    public YGValue GetHeight()
    {
        CheckDisposed();
        return node->GetHeight();
    }

    /// <inheritdoc cref="YGNode.SetMinWidth(float)" />
    public void SetMinWidth(float minWidth)
    {
        CheckDisposed();
        node->SetMinWidth(minWidth);
    }

    /// <inheritdoc cref="YGNode.SetMinWidthPercent(float)" />
    public void SetMinWidthPercent(float minWidth)
    {
        CheckDisposed();
        node->SetMinWidthPercent(minWidth);
    }

    /// <inheritdoc cref="YGNode.GetMinWidth()" />
    public YGValue GetMinWidth()
    {
        CheckDisposed();
        return node->GetMinWidth();
    }

    /// <inheritdoc cref="YGNode.SetMinHeight(float)" />
    public void SetMinHeight(float minHeight)
    {
        CheckDisposed();
        node->SetMinHeight(minHeight);
    }

    /// <inheritdoc cref="YGNode.SetMinHeightPercent(float)" />
    public void SetMinHeightPercent(float minHeight)
    {
        CheckDisposed();
        node->SetMinHeightPercent(minHeight);
    }

    /// <inheritdoc cref="YGNode.GetMinHeight()" />
    public YGValue GetMinHeight()
    {
        CheckDisposed();
        return node->GetMinHeight();
    }

    /// <inheritdoc cref="YGNode.SetMaxWidth(float)" />
    public void SetMaxWidth(float maxWidth)
    {
        CheckDisposed();
        node->SetMaxWidth(maxWidth);
    }

    /// <inheritdoc cref="YGNode.SetMaxWidthPercent(float)" />
    public void SetMaxWidthPercent(float maxWidth)
    {
        CheckDisposed();
        node->SetMaxWidthPercent(maxWidth);
    }

    /// <inheritdoc cref="YGNode.GetMaxWidth()" />
    public YGValue GetMaxWidth()
    {
        CheckDisposed();
        return node->GetMaxWidth();
    }

    /// <inheritdoc cref="YGNode.SetMaxHeight(float)" />
    public void SetMaxHeight(float maxHeight)
    {
        CheckDisposed();
        node->SetMaxHeight(maxHeight);
    }

    /// <inheritdoc cref="YGNode.SetMaxHeightPercent(float)" />
    public void SetMaxHeightPercent(float maxHeight)
    {
        CheckDisposed();
        node->SetMaxHeightPercent(maxHeight);
    }

    /// <inheritdoc cref="YGNode.GetMaxHeight()" />
    public YGValue GetMaxHeight()
    {
        CheckDisposed();
        return node->GetMaxHeight();
    }

    /// <inheritdoc cref="YGNode.CalculateLayout(Vector2, Direction)" />
    public void CalculateLayout(Vector2 ownerSize, Direction ownerDirection = Direction.LTR)
    {
        CheckDisposed();
        node->CalculateLayout(ownerSize, ownerDirection);
    }

    /// <inheritdoc cref="YGNode.AppendChild(YGNode*)" />
    public void AppendChild(YGNode* child)
    {
        CheckDisposed();
        node->AppendChild(child);
    }

    /// <inheritdoc cref="YGNode.GetComputedSize()" />
    public Vector2 GetComputedSize()
    {
        CheckDisposed();
        return node->GetComputedSize();
    }

    /// <inheritdoc cref="YGNode.GetComputedPosition()" />
    public Vector2 GetComputedPosition()
    {
        CheckDisposed();
        return node->GetComputedPosition();
    }

    #endregion

    public static implicit operator YGNode*(Node imageNode) => imageNode.node;
}
