namespace HaselCommon.Utils;

public class LimitedQueue<T>(int limit) : Queue<T>
{
    public new void Enqueue(T item)
    {
        base.Enqueue(item);

        if (Count > limit)
            Dequeue();
    }
}
