using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.LevelPos)] // n x
public class LevelPosPayload : HaselMacroPayload
{
    public LevelPosPayload() : base()
    {
    }

    public LevelPosPayload(uint levelId) : base()
    {
        LevelId = new IntegerExpression(levelId);
    }

    public BaseExpression? LevelId { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        if (LevelId == null)
            return new();

        var level = GetRow<Level>((uint)LevelId.ResolveNumber(localParameters));
        if (level == null)
            return new();

        var map = GetRow<Map>(level.Map.Row);
        if (map == null)
            return new();

        var placeName = GetRow<PlaceName>(map.PlaceName.Row);
        if (placeName == null)
            return new();

        return HaselSeString.FromAddon(1637).Resolve([
            placeName.Name,
            map.ConvertRawToMapPosX(level.X),
            map.ConvertRawToMapPosY(level.Z),
        ]);
    }
}
