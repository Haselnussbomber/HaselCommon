using System.Diagnostics.CodeAnalysis;

namespace HaselCommon.Extensions;

public static class ValueEnumerableExtensions
{
    public static int IndexOf<TEnumerator, TSource>(
        this ValueEnumerable<TEnumerator, TSource> source,
        Predicate<TSource> predicate)
        where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
    {
        if (predicate == null)
            return -1;

        var i = 0;

        foreach (var obj in source)
        {
            if (predicate(obj))
                return i;

            ++i;
        }

        return -1;
    }

    public static int IndexOf<TEnumerator, TSource>(
        this ValueEnumerable<TEnumerator, TSource> source,
        TSource needle)
        where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
    {
        if (needle == null)
            return -1;

        var i = 0;

        foreach (var obj in source)
        {
            if (needle.Equals(obj))
                return i;

            ++i;
        }

        return -1;
    }

    public static bool TryGetFirst<TEnumerator, TSource>(
        this ValueEnumerable<TEnumerator, TSource> source,
        Predicate<TSource> predicate,
        [NotNullWhen(returnValue: true)] out TSource? result)
        where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
    {
        if (predicate == null)
        {
            result = default;
            return false;
        }

        using var e = source.Enumerator;

        if (e.TryGetSpan(out var span))
        {
            // faster iteration
            foreach (var item in span)
            {
                if (item != null && predicate(item))
                {
                    result = item;
                    return true;
                }
            }
        }
        else
        {
            while (e.TryGetNext(out var item))
            {
                if (item != null && predicate(item))
                {
                    result = item;
                    return true;
                }
            }
        }

        result = default;
        return false;
    }

    public static bool TryGetFirst<TEnumerator, TSource>(
        this ValueEnumerable<TEnumerator, TSource> source,
        Predicate<TSource> predicate,
        [NotNullWhen(returnValue: true)] out TSource? result,
        out int index)
        where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
    {
        index = 0;

        if (predicate == null)
        {
            result = default;
            return false;
        }

        using var e = source.Enumerator;

        if (e.TryGetSpan(out var span))
        {
            // faster iteration
            foreach (var item in span)
            {
                if (item != null && predicate(item))
                {
                    result = item;
                    return true;
                }

                index++;
            }
        }
        else
        {
            while (e.TryGetNext(out var item))
            {
                if (item != null && predicate(item))
                {
                    result = item;
                    return true;
                }

                index++;
            }
        }

        result = default;
        return false;
    }
}
