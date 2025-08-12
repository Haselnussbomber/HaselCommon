using System.Globalization;

namespace HaselCommon;

public partial class CommonTranslations
{
    public string FormatCoordsXY(float x, float y)
    {
        return string.Format(
            CoordsXY,
            x.ToString("0.0", CultureInfo.InvariantCulture),
            y.ToString("0.0", CultureInfo.InvariantCulture));
    }
}
