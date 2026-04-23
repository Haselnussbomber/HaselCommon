using Microsoft.CodeAnalysis;

namespace HaselCommon.TranslationGenerator;

public record SourceRecord(INamedTypeSymbol Symbol, Dictionary<string, Dictionary<string, string>> Translations)
{
    public string FullyQualifiedMetadataName => Symbol.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
}
