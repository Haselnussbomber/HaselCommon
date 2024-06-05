using System.Collections.Generic;

namespace HaselCommon.Utils;

public class ReverseComparer<T> : IComparer<T> where T : IComparable<T>
{
    public int Compare(T? x, T? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        return y.CompareTo(x);
    }
}
