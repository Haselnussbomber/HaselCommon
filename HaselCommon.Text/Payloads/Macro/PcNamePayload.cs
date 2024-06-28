using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.PcName)] // n x
public class PcNamePayload : MacroPayload
{
    public Expression? EntityId { get; set; }

    [TerminatorExpression]
    private Expression? Terminator { get; set; }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (EntityId == null)
            return new();

        var entityId = (uint)EntityId.ResolveNumber(localParameters);

        return GetNameForEntityId(entityId);
    }

    private static unsafe string GetNameForEntityId(uint entityId)
    {
        var chara = CharacterManager.Instance()->LookupBattleCharaByEntityId(entityId);
        return chara == null
            ? string.Empty
            : MemoryHelper.ReadStringNullTerminated((nint)chara->Character.GameObject.GetName());
    }
}
