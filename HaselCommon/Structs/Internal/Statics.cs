using FFXIVClientStructs.FFXIV.Client.System.String;

namespace HaselCommon.Structs.Internal;

internal unsafe partial struct Statics
{
    [MemberFunction("80 F9 07 77 10")]
    internal static partial bool IsGatheringPointRare(byte gatheringPointType);

    [MemberFunction("E8 ?? ?? ?? ?? 48 8D 4D A7 E8 ?? ?? ?? ?? EB 18")]
    internal static partial Utf8String* Utf8StringReplace(Utf8String* str, Utf8String* toFind, Utf8String* replacement);

    [MemberFunction("E8 ?? ?? ?? ?? 80 7D 97 00")]
    internal static partial int Utf8StringIndexOf(Utf8String* str, Utf8String* toFind, ulong startIdx = 0);
}
