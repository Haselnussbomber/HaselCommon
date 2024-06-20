using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Extensions;

public static class LuminaSeStringExtensions
{
    public static string ExtractText(this SeString str)
        => new ReadOnlySeStringSpan(str.RawData).ExtractText();
}
