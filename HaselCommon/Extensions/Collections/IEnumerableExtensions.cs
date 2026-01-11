namespace HaselCommon.Extensions;

public static class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public int IndexOf(Predicate<T> predicate)
        {
            var i = 0;

            foreach (var obj in enumerable)
            {
                if (predicate(obj))
                    return i;

                ++i;
            }

            return -1;
        }

        public bool Contains(Predicate<T> predicate)
        {
            foreach (var element in enumerable)
            {
                if (predicate(element))
                    return true;
            }

            return false;
        }

        public void ForEach(Action<T> action)
        {
            foreach (var element in enumerable)
                action(element);
        }

        public IEnumerable<(int Score, T Value)> FuzzyMatch(string term, Func<T, string> valueExtractor, FuzzyMatcherMode matchMode = FuzzyMatcherMode.Fuzzy)
        {
            var results = new PriorityQueue<T, int>(enumerable.Count(), new ReverseComparer<int>());

            foreach (var value in enumerable)
            {
                var score = FuzzyMatcher.FuzzyScore(valueExtractor(value), term, matchMode);
                if (score != 0)
                    results.Enqueue(value, score);
            }

            while (results.TryDequeue(out var result, out var score))
                yield return (score, result);
        }
    }

    extension<T>(IEnumerable<T> enumerable) where T : notnull
    {
        public int IndexOf(T needle)
        {
            var i = 0;

            foreach (var obj in enumerable)
            {
                if (needle.Equals(obj))
                    return i;

                ++i;
            }

            return -1;
        }
    }

    extension<T>(IEnumerable<T> enumerable) where T : struct
    {
        public bool TryGetFirst(Predicate<T> predicate, out T result, out int index)
        {
            using var e = enumerable.GetEnumerator();
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
