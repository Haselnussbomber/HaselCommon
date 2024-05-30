using Microsoft.CodeAnalysis;

namespace HaselCommon.HookGenerator.Models;

internal sealed record ParameterInfo(
    string Name,
    string Type,
    string? DefaultValue,
    RefKind RefKind);
