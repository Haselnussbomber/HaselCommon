using Microsoft.CodeAnalysis;

namespace HaselCommon.HookGenerator.Extensions;

public static class RefKindExtensions
{
    public static string GetParameterPrefix(this RefKind refKind)
    {
        return refKind switch
        {
            RefKind.In => "in ",
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            RefKind.RefReadOnlyParameter => "ref readonly ",
            _ => "",
        };
    }
}
