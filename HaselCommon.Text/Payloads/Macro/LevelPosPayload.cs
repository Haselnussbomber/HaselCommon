using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.LevelPos)] // n x
public class LevelPosPayload : MacroPayload
{
    public LevelPosPayload() : base()
    {
    }

    public LevelPosPayload(uint levelId) : base()
    {
        LevelId = new IntegerExpression(levelId);
    }

    public Expression? LevelId { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
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

        return SeString.FromAddon(1637).Resolve([
            placeName.Name,
            map.ConvertRawToMapPosX(level.X),
            map.ConvertRawToMapPosY(level.Z),
        ]);
    }
}
