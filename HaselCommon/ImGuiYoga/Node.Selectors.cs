using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExCSS;

namespace HaselCommon.ImGuiYoga;

// TODO: add closest(selectors) https://dom.spec.whatwg.org/#dom-element-closest
public partial class Node
{
    [GeneratedRegex(@"\s")]
    private static partial Regex RegexWhitespace();

    public T? QuerySelector<T>(string selector) where T : Node
        => QuerySelector(selector) is T typed ? typed : null;

    public Node? QuerySelector(string selector)
        => QuerySelector(NodeParser.StylesheetParser.ParseSelector(selector));

    public IReadOnlyList<Node> QuerySelectorAll(string selector)
        => QuerySelectorAll(NodeParser.StylesheetParser.ParseSelector(selector));

    public Node? QuerySelector(ISelector selector, bool recursive = true) // TODO: hide recursive from public api?
        => FindNode(child => child.Matches(selector), recursive);

    public IReadOnlyList<Node> QuerySelectorAll(ISelector selector, bool recursive = true)
        => FindAllNodes(child => child.Matches(selector), recursive);

    public bool Matches(string selector)
        => Matches(NodeParser.StylesheetParser.ParseSelector(selector));

    // https://www.w3.org/TR/selectors-4/#overview
    // Note: ExCSS only supports CSS 3, but the links here are for Selectors Level 4.
    // The spec links are just for reference and not necessarily the reason of the implementation.
    public bool Matches(ISelector selector)
        => selector switch
        {
            // § 5.2 Universal selector
            // https://www.w3.org/TR/selectors-4/#the-universal-selector
            AllSelector => true,

            // § 5.1 Type (tag name) selector
            // https://www.w3.org/TR/selectors-4/#type-selectors
            TypeSelector typeSelector => false, // TODO: we don't have tag names yet.. maybe we should carry the ElementRegistry over to the Document?

            // § 6.6 Class selectors
            // https://www.w3.org/TR/selectors-4/#class-html
            ClassSelector classSelector => ClassList.Contains(classSelector.Class),

            // § 6.7 ID selectors
            // https://www.w3.org/TR/selectors-4/#id-selectors
            IdSelector idSelector => Id == idSelector.Id,

            // Compound selector
            // https://www.w3.org/TR/selectors-4/#compound
            CompoundSelector compoundSelector => compoundSelector.All(Matches),

            // § 6.1 Attribute presence and value selectors
            // https://www.w3.org/TR/selectors-4/#attribute-representation
            AttrAvailableSelector attrAvailableSelector => Attributes.ContainsKey(attrAvailableSelector.Attribute),
            AttrMatchSelector attrMatchSelector => Attributes[attrMatchSelector.Attribute] == attrMatchSelector.Value,
            AttrNotMatchSelector attrNotMatchSelector => Attributes[attrNotMatchSelector.Attribute] != attrNotMatchSelector.Value, // is this even a thing?
            AttrListSelector attrListSelector => RegexWhitespace().Split(Attributes[attrListSelector.Attribute]).Contains(attrListSelector.Value),
            AttrBeginsSelector attrBeginsSelector => Attributes[attrBeginsSelector.Attribute].StartsWith(attrBeginsSelector.Value),
            AttrEndsSelector attrEndsSelector => Attributes[attrEndsSelector.Attribute].EndsWith(attrEndsSelector.Value),
            AttrContainsSelector attrContainsSelector => Attributes[attrContainsSelector.Attribute].Contains(attrContainsSelector.Value),

            // § 14.3.1. :nth-child() pseudo-class
            // https://www.w3.org/TR/selectors-4/#the-nth-child-pseudo
            FirstChildSelector firstChildSelector => MatchesNthChildSelector(firstChildSelector),

            // § 3.5. Pseudo-classes
            // https://www.w3.org/TR/selectors-4/#pseudo-classes
            PseudoClassSelector pseudoClassSelector => MatchesPseudoClassSelector(pseudoClassSelector),

            ComplexSelector complexSelector => MatchesComplexSelector(complexSelector),

            _ => throw new NotImplementedException($"Selector \"{selector.GetType()}\" is not supported"),
        };

    public Node? FindNode(Predicate<Node> predicate, bool recursive = true)
    {
        foreach (var child in Children)
        {
            if (predicate(child))
                return child;

            if (recursive)
            {
                var node = child.FindNode(predicate, recursive);
                if (node != null)
                    return node;
            }
        }

        return null;
    }

    public IReadOnlyList<Node> FindAllNodes(Predicate<Node> predicate, bool recursive = true)
    {
        var result = new List<Node>();

        foreach (var child in Children)
        {
            if (predicate(child))
                result.Add(child);

            if (recursive)
            {
                result.AddRange(child.FindAllNodes(predicate));
            }
        }

        return result;
    }

    public Node? FindParent(string selector)
        => FindParent(NodeParser.StylesheetParser.ParseSelector(selector));

    public Node? FindParent(ISelector selector)
    {
        var node = this;

        while ((node = node.Parent) != null)
        {
            if (node.Matches(selector))
                return node;
        }

        return null;
    }

    // § 14.3.1 :nth-child() pseudo-class
    // https://www.w3.org/TR/selectors-4/#the-nth-child-pseudo
    // TODO: support "of S" https://www.w3.org/TR/selectors-4/#the-nth-child-pseudo
    private bool MatchesNthChildSelector(FirstChildSelector selector)
    {
        // Service.Get<IPluginLog>().Verbose("{displayName}.MatchesNthChildSelector({selector}) => offset: {offset}, step: {step}", DisplayName, selector.ToCss(), selector.Offset, selector.Step);

        var index = Parent?.IndexOf(this) ?? -1;
        if (index == -1)
            return false;

        if (index < selector.Offset)
            return false;

        index -= selector.Offset;

        if (selector.Step != 0)
            index %= selector.Step;

        return index == 0;
    }

    private bool MatchesPseudoClassSelector(PseudoClassSelector selector)
    {
        // Service.Get<IPluginLog>().Verbose("{displayName}.MatchesPseudoClassSelector({selector}) => class: {class}", DisplayName, selector.ToCss(), selector.Class);

        // § 14.3.3 :first-child pseudo-class
        // https://www.w3.org/TR/selectors-4/#the-first-child-pseudo
        if (selector.Class == PseudoClassNames.FirstChild)
        {
            var index = Parent?.IndexOf(this) ?? -1;
            return index == 0;
        }

        // § 14.3.4 :last-child pseudo-class
        // https://www.w3.org/TR/selectors-4/#the-last-child-pseudo
        if (selector.Class == PseudoClassNames.LastChild)
        {
            var parent = Parent;
            return parent != null && parent.IndexOf(this) == parent.Count - 1;
        }

        // § 14.3.5 :only-child pseudo-class
        // https://www.w3.org/TR/selectors-4/#the-only-child-pseudo
        if (selector.Class == PseudoClassNames.OnlyChild)
        {
            var index = Parent?.Count ?? 0;
            return index == 1;
        }

        // § 4.3. The Negation (Matches-None) Pseudo-class: :not()
        // https://www.w3.org/TR/selectors-4/#negation
        if (selector.Class.StartsWith(PseudoClassNames.Not + "(") && selector.Class.EndsWith(')')) // not(...)
        {
            return !Matches(selector.Class[4..^1]);
        }

        // § 4.5. The Relational Pseudo-class: :has()
        // https://www.w3.org/TR/selectors-4/#relational
        if (selector.Class.StartsWith(PseudoClassNames.Has + "(") && selector.Class.EndsWith(')')) // has(...)
        {
            return QuerySelector(selector.Class[4..^1]) != null;
        }

        if (selector.Class == PseudoClassNames.Hover)
        {
            if (!Interactive) Interactive = true;
            return IsHovered == true;
        }

        throw new NotImplementedException($"Pseudo class selector \"{selector.Class}\" is not supported");
    }

    private bool MatchesComplexSelector(ComplexSelector selector)
    {
        // Service.Get<IPluginLog>().Verbose("{displayName}.MatchesComplexSelector({selector})", DisplayName, selector.ToCss());

        var node = this;

        foreach (var sel in selector.Reverse())
        {
            // Service.Get<IPluginLog>().Verbose("  => {delimiter} {selector}", sel.Delimiter, sel.Selector.ToCss());

            if (string.IsNullOrEmpty(sel.Delimiter))
            {
                if (!node.Matches(sel.Selector))
                    return false;
                continue;
            }

            // " "
            if (sel.Delimiter == Combinators.Descendent)
            {
                if ((node = node.FindParent(sel.Selector)) == null)
                    return false;
                continue;
            }

            // ">"
            if (sel.Delimiter == Combinators.Child)
            {
                if ((node = node.Parent) == null || !node.Matches(sel.Selector))
                    return false;
                continue;
            }

            // "+"
            if (sel.Delimiter == Combinators.Adjacent)
            {
                if ((node = node.PreviousSibling) == null || !node.Matches(sel.Selector))
                    return false;
                continue;
            }

            // "~"
            if (sel.Delimiter == Combinators.Sibling)
            {
                var found = false;
                foreach (var sibling in node.PreviousSiblings)
                {
                    if (found |= sibling.Matches(sel.Selector))
                        break;
                }
                if (found)
                    continue;
                return false;
            }

            throw new NotImplementedException($"Delimiter \"{sel.Delimiter}\" is not supported");
        }

        return true;
    }
}
