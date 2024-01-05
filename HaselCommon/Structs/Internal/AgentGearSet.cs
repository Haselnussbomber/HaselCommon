namespace HaselCommon.Structs.Internal;

// ctor "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 56 48 83 EC 60 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B F1"
[StructLayout(LayoutKind.Explicit, Size = 0xB00)]
internal unsafe partial struct AgentGearSet
{
    internal enum ContextMenuGlamourCallbackAction
    {
        Link = 20,
        ChangeLink = 21,
        Unlink = 22,
    }

    [MemberFunction("40 53 48 83 EC 20 8B DA 41 83 F8 14")]
    internal partial void ContextMenuGlamourCallback(uint gearsetId, ContextMenuGlamourCallbackAction action);
}
