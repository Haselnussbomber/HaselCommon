using System.Globalization;

namespace HaselCommon.Extensions;

public static class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public int IndexOf(Predicate<T> predicate)
        {
            var i = 0;

            foreach (var obj in source)
            {
                if (predicate(obj))
                    return i;

                ++i;
            }

            return -1;
        }

        public bool Contains(Predicate<T> predicate)
        {
            foreach (var element in source)
            {
                if (predicate(element))
                    return true;
            }

            return false;
        }

        public void ForEach(Action<T> action)
        {
            foreach (var element in source)
                action(element);
        }

        public IEnumerable<(int Score, T Value)> FuzzyMatch(string term, Func<T, string> valueExtractor, CultureInfo cultureInfo, FuzzyMatcher.Mode matchMode = FuzzyMatcher.Mode.Fuzzy)
        {
            var results = new PriorityQueue<T, int>(source.Count(), new ReverseComparer<int>());

            foreach (var value in source)
            {
                if (valueExtractor(value).FuzzyMatches(term, matchMode, cultureInfo, out var score))
                    results.Enqueue(value, score);
            }

            while (results.TryDequeue(out var result, out var score))
                yield return (score, result);
        }
    }

    extension<T>(IEnumerable<T> source)
        where T : notnull
    {
        public int IndexOf(T needle)
        {
            var i = 0;

            foreach (var obj in source)
            {
                if (needle.Equals(obj))
                    return i;

                ++i;
            }

            return -1;
        }
    }

    extension<T>(IEnumerable<T> source)
        where T : struct
    {
        public bool TryGetFirst(Predicate<T> predicate, out T result, out int index)
        {
            using var e = source.GetEnumerator();
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
    }
}
