#define BORDER_MATCHING

using System.Runtime.CompilerServices;

namespace HaselCommon.Utils;

// copied from Dalamud:
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Utility/FuzzyMatcher.cs

public readonly ref struct FuzzyMatcher
{
    private static readonly (int, int)[] EmptySegArray = [];

    private readonly string _needleString = string.Empty;
    private readonly ReadOnlySpan<char> _needleSpan = [];
    private readonly int _needleFinalPosition = -1;
    private readonly (int Start, int End)[] _needleSegments = EmptySegArray;
    private readonly MatchMode _mode = MatchMode.Simple;

    public FuzzyMatcher(string term, MatchMode matchMode)
    {
        _needleString = term;
        _needleSpan = _needleString.AsSpan();
        _needleFinalPosition = _needleSpan.Length - 1;
        _mode = matchMode;

        _needleSegments = matchMode switch
        {
            MatchMode.FuzzyParts => FindNeedleSegments(_needleSpan),
            MatchMode.Fuzzy or MatchMode.Simple => EmptySegArray,
            _ => throw new ArgumentOutOfRangeException(nameof(matchMode), matchMode, null),
        };
    }

    private static (int Start, int End)[] FindNeedleSegments(ReadOnlySpan<char> span)
    {
        var segments = new List<(int, int)>();
        var wordStart = -1;

        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] is not ' ' and not '\u3000')
            {
                if (wordStart < 0)
                {
                    wordStart = i;
                }
            }
            else if (wordStart >= 0)
            {
                segments.Add((wordStart, i - 1));
                wordStart = -1;
            }
        }

        if (wordStart >= 0)
        {
            segments.Add((wordStart, span.Length - 1));
        }

        return [.. segments];
    }

    public int Matches(string value)
    {
        if (_needleFinalPosition < 0)
        {
            return 0;
        }

        if (_mode == MatchMode.Simple)
        {
            return value.Contains(_needleString) ? 1 : 0;
        }

        var haystack = value.AsSpan();

        if (_mode == MatchMode.Fuzzy)
        {
            return GetRawScore(haystack, 0, _needleFinalPosition);
        }

        if (_mode == MatchMode.FuzzyParts)
        {
            if (_needleSegments.Length < 2)
            {
                return GetRawScore(haystack, 0, _needleFinalPosition);
            }

            var total = 0;
            for (var i = 0; i < _needleSegments.Length; i++)
            {
                var (start, end) = _needleSegments[i];
                var cur = GetRawScore(haystack, start, end);
                if (cur == 0)
                {
                    return 0;
                }

                total += cur;
            }

            return total;
        }

        return 8;
    }

    public int MatchesAny(params string[] values)
    {
        var max = 0;
        for (var i = 0; i < values.Length; i++)
        {
            var cur = Matches(values[i]);
            if (cur > max)
            {
                max = cur;
            }
        }

        return max;
    }

    private int GetRawScore(ReadOnlySpan<char> haystack, int needleStart, int needleEnd)
    {
        var (startPos, gaps, consecutive, borderMatches, endPos) = FindForward(haystack, needleStart, needleEnd);
        if (startPos < 0)
        {
            return 0;
        }

        var needleSize = needleEnd - needleStart + 1;

        var score = CalculateRawScore(needleSize, startPos, gaps, consecutive, borderMatches);

        (startPos, gaps, consecutive, borderMatches) = FindReverse(haystack, endPos, needleStart, needleEnd);
        var revScore = CalculateRawScore(needleSize, startPos, gaps, consecutive, borderMatches);

        return int.Max(score, revScore);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalculateRawScore(int needleSize, int startPos, int gaps, int consecutive, int borderMatches)
    {
        var score = 100
                    + needleSize * 3
                    + borderMatches * 3
                    + consecutive * 5
                    - startPos
                    - gaps * 2;
        if (startPos == 0)
            score += 5;
        return score < 1 ? 1 : score;
    }

    private (int StartPos, int Gaps, int Consecutive, int BorderMatches, int HaystackIndex) FindForward(
        ReadOnlySpan<char> haystack, int needleStart, int needleEnd)
    {
        var needleIndex = needleStart;
        var lastMatchIndex = -10;

        var startPos = 0;
        var gaps = 0;
        var consecutive = 0;
        var borderMatches = 0;

        for (var haystackIndex = 0; haystackIndex < haystack.Length; haystackIndex++)
        {
            if (haystack[haystackIndex] == _needleSpan[needleIndex])
            {
#if BORDER_MATCHING
                if (haystackIndex > 0)
                {
                    if (!char.IsLetterOrDigit(haystack[haystackIndex - 1]))
                    {
                        borderMatches++;
                    }
                }
#endif

                needleIndex++;

                if (haystackIndex == lastMatchIndex + 1)
                {
                    consecutive++;
                }

                if (needleIndex > needleEnd)
                {
                    return (startPos, gaps, consecutive, borderMatches, haystackIndex);
                }

                lastMatchIndex = haystackIndex;
            }
            else
            {
                if (needleIndex > needleStart)
                {
                    gaps++;
                }
                else
                {
                    startPos++;
                }
            }
        }

        return (-1, 0, 0, 0, 0);
    }

    private (int StartPos, int Gaps, int Consecutive, int BorderMatches) FindReverse(
        ReadOnlySpan<char> haystack, int haystackLastMatchIndex, int needleStart, int needleEnd)
    {
        var needleIndex = needleEnd;
        var revLastMatchIndex = haystack.Length + 10;

        var gaps = 0;
        var consecutive = 0;
        var borderMatches = 0;

        for (var haystackIndex = haystackLastMatchIndex; haystackIndex >= 0; haystackIndex--)
        {
            if (haystack[haystackIndex] == _needleSpan[needleIndex])
            {
#if BORDER_MATCHING
                if (haystackIndex > 0)
                {
                    if (!char.IsLetterOrDigit(haystack[haystackIndex - 1]))
                    {
                        borderMatches++;
                    }
                }
#endif

                needleIndex--;

                if (haystackIndex == revLastMatchIndex - 1)
                {
                    consecutive++;
                }

                if (needleIndex < needleStart)
                {
                    return (haystackIndex, gaps, consecutive, borderMatches);
                }

                revLastMatchIndex = haystackIndex;
            }
            else
            {
                gaps++;
            }
        }

        return (-1, 0, 0, 0);
    }
}

public enum MatchMode
{
    Simple,
    Fuzzy,
    FuzzyParts,
}
