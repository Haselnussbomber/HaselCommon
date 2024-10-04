using System.Runtime.CompilerServices;
using Lumina.Text;

namespace HaselCommon.Extensions.Strings;

public static class LuminaSeStringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ExtractText(this SeString str)
    {
        // removes soft hyphen which would be displayed in ImGui
        return str.AsReadOnly().ExtractText().Replace("\u00AD", "");
    }
}
