using System.Globalization;

namespace HaselCommon.Extensions;

public static class IEnumerableExtensions
{
    public static int IndexOf<T>(this IEnumerable<T> array, Predicate<T> predicate)
    {
        var i = 0;

        foreach (var obj in array)
        {
            if (predicate(obj))
                return i;

            ++i;
        }

        return -1;
    }

    public static int IndexOf<T>(this IEnumerable<T> array, T needle) where T : notnull
    {
        var i = 0;

        foreach (var obj in array)
        {
            if (needle.Equals(obj))
                return i;

            ++i;
        }

        return -1;
    }

    public static bool Contains<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        foreach (var element in source)
        {
            if (predicate(element))
                return true;
        }

        return false;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source)
            action(element);
    }

    public static bool TryGetFirst<T>(this IEnumerable<T> values, Predicate<T> predicate, out T result, out int index) where T : struct
    {
        using var e = values.GetEnumerator();
        index = 0;

        while (e.MoveNext())
        {
            if (predicate(e.Current))
            {
                result = e.Current;
                return true;
            }

            index++;
        }

        result = default;
        return false;
    }

    public static IEnumerable<(int Score, T Value)> FuzzyMatch<T>(this IEnumerable<T> values, string term, Func<T, string> valueExtractor, CultureInfo cultureInfo, FuzzyMatcher.Mode matchMode = FuzzyMatcher.Mode.Fuzzy)
    {
        var results = new PriorityQueue<T, int>(values.Count(), new ReverseComparer<int>());

        foreach (var value in values)
        {
            if (valueExtractor(value).FuzzyMatches(term, matchMode, cultureInfo, out var score))
                results.Enqueue(value, score);
        }

        while (results.TryDequeue(out var result, out var score))
            yield return (score, result);
    }
}
