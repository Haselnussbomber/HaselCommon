using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.IfSelf)] // n . . x
public class IfSelfPayload : HaselMacroPayload
{
    public BaseExpression? ObjectId { get; set; }
    public BaseExpression? CaseTrue { get; set; }
    public BaseExpression? CaseFalse { get; set; }

    [TerminatorExpression]
    private BaseExpression? Terminator { get; set; }

    public override unsafe HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (ObjectId == null)
            return new();

        var isSelf = (uint)ObjectId.ResolveNumber(localParameterData) == PlayerState.Instance()->ObjectId;
        return (isSelf ? CaseTrue : CaseFalse)?.ResolveString(localParameterData) ?? new();
    }
}
