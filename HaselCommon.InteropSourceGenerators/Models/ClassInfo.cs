using HaselCommon.InteropGenerator;
using HaselCommon.InteropSourceGenerators.Extensions;
using LanguageExt;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static LanguageExt.Prelude;

namespace HaselCommon.InteropSourceGenerators.Models;

internal sealed record ClassInfo(string Name, string Namespace)
{
    public static Validation<DiagnosticInfo, ClassInfo> GetFromSyntax(ClassDeclarationSyntax classSyntax)
    {
        return Success<DiagnosticInfo, ClassInfo>(
            new ClassInfo(
                classSyntax.GetNameWithTypeDeclarationList(),
                classSyntax.GetContainingFileScopedNamespace()));
    }

    public void RenderStart(IndentedStringBuilder builder)
    {
        builder.AppendLine("// <auto-generated/>");

        if (!string.IsNullOrWhiteSpace(Namespace))
        {
            builder.AppendLine();
            builder.AppendLine($"namespace {Namespace};");
        }

        builder.AppendLine();
        builder.AppendLine($"public unsafe partial class {Name}");
        builder.AppendLine("{");
        builder.Indent();
    }

    public void RenderEnd(IndentedStringBuilder builder)
    {
        builder.DecrementIndent();
        builder.AppendLine("}");
    }
}
