using System.Globalization;

namespace HaselCommon;

[AutoConstruct]
[RegisterSingleton]
[RegisterSingleton<ITranslationProvider>(Duplicate = DuplicateStrategy.Append)]
public partial class CommonTranslations : ITranslationProvider
{
    private readonly ISeStringEvaluator _seStringEvaluator;
    private readonly LanguageProvider _languageProvider;

    public ReadOnlySpan<byte> FormatCoordsXY(float x, float y)
    {
        ReadOnlySpan<char> format = ['0', '.', '0'];

        Span<byte> xBuffer = stackalloc byte[32];
        if (!x.TryFormat(xBuffer, out var xBytesWritten, format, CultureInfo.InvariantCulture))
            return ""u8;

        Span<byte> yBuffer = stackalloc byte[32];
        if (!y.TryFormat(yBuffer, out var yBytesWritten, format, CultureInfo.InvariantCulture))
            return ""u8;

        return ReadOnlySeString.Format($"X: {xBuffer[..xBytesWritten]}, Y: {yBuffer[..yBytesWritten]}");
    }

    public string FormatCoordsXYString(float x, float y)
    {
        return string.Format(
            CoordsXYString,
            x.ToString("0.0", CultureInfo.InvariantCulture),
            y.ToString("0.0", CultureInfo.InvariantCulture));
    }
}
