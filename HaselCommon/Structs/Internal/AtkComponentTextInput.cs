using FFXIVClientStructs.FFXIV.Component.GUI;
using HaselCommon.Utils;

namespace HaselCommon.Structs.Internal;

internal readonly unsafe struct HAtkComponentTextInput
{
    internal unsafe delegate void TriggerRedrawDelegate(AtkComponentTextInput* input);
    internal static readonly Lazy<TriggerRedrawDelegate> TriggerRedraw
        = new(() => MemoryUtils.GetDelegateForSignature<TriggerRedrawDelegate>("E8 ?? ?? ?? ?? 48 0F BF 56"));
}
