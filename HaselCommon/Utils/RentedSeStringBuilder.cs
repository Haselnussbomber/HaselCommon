namespace HaselCommon.Utils;

public readonly struct RentedSeStringBuilder() : IDisposable
{
    public SeStringBuilder Builder { get; } = SeStringBuilder.SharedPool.Get();
    public void Dispose() => SeStringBuilder.SharedPool.Return(Builder);
}
