namespace HaselCommon.Utils;

public class PooledListPolicy<T> : PooledObjectPolicy<List<T>>
{
    public override List<T> Create()
    {
        return [];
    }

    public override bool Return(List<T> list)
    {
        list.Clear();
        return true;
    }
}
