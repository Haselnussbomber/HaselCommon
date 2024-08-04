using Lumina.Text;

namespace HaselCommon.Extensions;

public static class LuminaSeStringExtensions
{
    public static string ExtractText(this SeString str)
        => str.AsReadOnly().ExtractText().Replace("\u00AD", "");
}
