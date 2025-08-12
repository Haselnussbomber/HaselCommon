namespace HaselCommon;

public interface ITranslationProvider
{
    bool TryGetTranslation(string key, out string text);
}
