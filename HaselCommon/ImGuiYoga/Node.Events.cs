using HaselCommon.ImGuiYoga.Events;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

public partial class Node
{
    public override void DispatchEvent(Event evt)
    {
        UpdateStyles(evt);
        base.DispatchEvent(evt);

        if (evt.Bubbles) // can be stopped by the nodes implementation of OnEvent through evt.StopPropagation()
        {
            Parent?.DispatchEvent(evt); // calls this function of the parent
        }
    }

    private void UpdateStyles(Event evt)
    {
        if (evt is not StyleChangedEvent styleChangedEvent)
            return;

        var propName = styleChangedEvent.PropertyName;
        var value = ResolveStyleValue(propName);
        if (string.IsNullOrEmpty(value) || value == "initial")
            return;

        switch (propName)
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

            case "gap":
                Gap = NodeParser.ParseFloat(value); // TODO: not handled like in css
                break;

            case "column-gap":
                GapColumn = NodeParser.ParseFloat(value);
                break;

            case "row-gap":
                GapRow = NodeParser.ParseFloat(value);
                break;
        }
    }
}
