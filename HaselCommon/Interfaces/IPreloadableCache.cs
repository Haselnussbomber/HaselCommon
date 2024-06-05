namespace HaselCommon.Interfaces;

public interface IPreloadableCache : ICache
{
    void Preload();
}
