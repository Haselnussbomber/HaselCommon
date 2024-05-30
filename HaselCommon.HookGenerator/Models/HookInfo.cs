namespace HaselCommon.HookGenerator.Models;

internal sealed record HookInfo(
    ClassInfo ClassInfo,
    MethodInfo MethodInfo,
    string? AddressName = null);
