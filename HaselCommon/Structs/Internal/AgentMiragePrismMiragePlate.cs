using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace HaselCommon.Structs.Internal;

[Agent(AgentId.MiragePrismMiragePlate)]
[StructLayout(LayoutKind.Explicit, Size = 0x350)]
internal partial struct AgentMiragePrismMiragePlate
{
    /// <remarks>
    /// The game usually checks <see cref="GameMain.IsInSanctuary"/> before calling this (and if false, it prints LogMessage 43: "Unable to apply glamour plates here.").
    /// </remarks>
    [MemberFunction("48 89 5C 24 ?? 57 48 83 EC 30 8B FA 66 44 89 4C 24")]
    public partial void OpenForGearset(int gearsetId, int glamourSetLink, ushort openerAddonId = 0);
}
