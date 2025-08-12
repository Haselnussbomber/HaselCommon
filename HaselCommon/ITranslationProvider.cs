namespace HaselCommon;

public interface ITranslationProvider
{
    bool TryGetTranslation(string key, out string text);
    bool TryGetTranslation(ReadOnlySpan<byte> key, out ReadOnlySpan<byte> text);
}
