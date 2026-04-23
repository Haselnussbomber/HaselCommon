using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace HaselCommon.TranslationGenerator;

internal static class AttributeDataExtensions {
    public static bool TryGetConstructorArgument<T>(this AttributeData attributeData, int index, [NotNullWhen(true)] out T? result)
    {
        if (attributeData.ConstructorArguments.Length <= index ||
            attributeData.ConstructorArguments[index].Value is not T argument)
        {
            result = default;
            return false;
        }

        result = argument;
        return true;
    }
}
