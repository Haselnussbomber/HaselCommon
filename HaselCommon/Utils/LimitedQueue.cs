using System.Collections.Generic;

namespace HaselCommon.Utils;

public sealed class LimitedQueue<T>(int limit) : Queue<T>
{
    public new void Enqueue(T item)
    {
        base.Enqueue(item);

        if (Count > limit)
            Dequeue();
    }
}
