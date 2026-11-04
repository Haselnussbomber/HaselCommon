using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HaselCommon.Generators;

[Generator]
public class DependencyInjectionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classesWithAttributes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetServiceRegistration(ctx))
            .Where(static m => m is not null);

        var methodsWithAttributes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is MethodDeclarationSyntax m && m.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetMethodRegistration(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider
            .Combine(classesWithAttributes.Collect())
            .Combine(methodsWithAttributes.Collect());

        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) =>
        {
            var compilation = source.Left.Left;
            var classes = source.Left.Right;
            var methods = source.Right;

            Execute(spc, compilation, classes!, methods!);
        });
    }

    private sealed class ServiceInfo
    {
        public INamedTypeSymbol ClassSymbol { get; }
        public ITypeSymbol ServiceType { get; }
        public bool IsSingleton { get; }
        public int DuplicateStrategy { get; }
        public string? FactoryMethod { get; }
        public List<ITypeSymbol> ConstructorParameters { get; }

        public ServiceInfo(
            INamedTypeSymbol classSymbol,
            ITypeSymbol serviceType,
            bool isSingleton,
            int duplicateStrategy,
            string? factoryMethod,
            List<ITypeSymbol> constructorParameters)
        {
            ClassSymbol = classSymbol;
            ServiceType = serviceType;
            IsSingleton = isSingleton;
            DuplicateStrategy = duplicateStrategy;
            FactoryMethod = factoryMethod;
            ConstructorParameters = constructorParameters;
        }
    }

    private static List<ServiceInfo>? GetServiceRegistration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return null;

        List<ServiceInfo>? list = null;

        foreach (var attr in classSymbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass == null) continue;

            var name = attrClass.Name;
            var isSingleton = name.StartsWith("RegisterSingleton");
            var isTransient = name.StartsWith("RegisterTransient");

            if (!isSingleton && !isTransient)
                continue;

            ITypeSymbol serviceType = classSymbol;
            if (attrClass.IsGenericType && attrClass.TypeArguments.Length == 1)
            {
                serviceType = attrClass.TypeArguments[0];
            }

            var duplicate = 0; // Skip
            string? factory = null;

            foreach (var named in attr.NamedArguments)
            {
                if (named.Key == "ServiceType" && named.Value.Value is ITypeSymbol st)
                {
                    serviceType = st;
                }
                else if (named.Key == "Duplicate" && named.Value.Value is int dup)
                {
                    duplicate = dup;
                }
                else if (named.Key == "Factory" && named.Value.Value is string fac)
                {
                    factory = fac;
                }
            }

            var ctorParams = factory == null ? GetConstructorParameters(classSymbol) : [];

            list ??= [];
            list.Add(new ServiceInfo(classSymbol, serviceType, isSingleton, duplicate, factory, ctorParams));
        }

        return list;
    }

    private static IMethodSymbol? GetMethodRegistration(GeneratorSyntaxContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
            return null;

        if (!methodSymbol.IsStatic)
            return null;

        foreach (var attr in methodSymbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "RegisterServicesAttribute")
                return methodSymbol;
        }

        return null;
    }

    private static List<ITypeSymbol> GetConstructorParameters(INamedTypeSymbol classSymbol)
    {
        // 1. Explicit public constructor
        var explicitCtor = classSymbol.Constructors
            .FirstOrDefault(c => !c.IsImplicitlyDeclared && c.DeclaredAccessibility == Accessibility.Public);

        if (explicitCtor != null)
        {
            return explicitCtor.Parameters.Select(p => p.Type).ToList();
        }
        
        // 2. Walk hierarchy for [AutoConstruct]
        var autoConstructClasses = new List<INamedTypeSymbol>();
        var parameters = new List<(string Name, ITypeSymbol Type)>();

        for (var current = classSymbol; current != null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            if (current.GetAttributes().Any(a => a.AttributeClass?.Name == "AutoConstructAttribute"))
            {
                autoConstructClasses.Add(current);
            }
            else
            {
                // Found the first non-AutoConstruct base class
                var baseCtor = current.Constructors
                    .FirstOrDefault(c => (c.DeclaredAccessibility == Accessibility.Protected || c.DeclaredAccessibility == Accessibility.Public) && c.Parameters.Length > 0);

                if (baseCtor != null)
                {
                    foreach (var p in baseCtor.Parameters)
                    {
                        parameters.Add((p.Name, p.Type));
                    }
                }
                break;
            }
        }
        autoConstructClasses.Reverse();

        foreach (var type in autoConstructClasses)
        {
            // Instance readonly fields without initializers
            foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
            {
                if (field.IsImplicitlyDeclared || field.IsStatic || !field.IsReadOnly || HasFieldInitializer(field))
                    continue;

                if (field.GetAttributes().Any(a => a.AttributeClass?.Name.StartsWith("AutoConstructIgnore") == true))
                    continue;

                var fieldName = field.Name.TrimStart('_');
                if (parameters.Any(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase) && SymbolEqualityComparer.Default.Equals(p.Type, field.Type)))
                    continue;

                parameters.Add((fieldName, field.Type));
            }

            // Parameters of [AutoPostConstruct] methods
            foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.GetAttributes().Any(a => a.AttributeClass?.Name.StartsWith("AutoConstructIgnore") == true))
                    continue;

                if (method.GetAttributes().Any(a => a.AttributeClass?.Name == "AutoPostConstructAttribute"))
                {
                    foreach (var mParam in method.Parameters)
                    {
                        if (parameters.Any(p => p.Name.Equals(mParam.Name, StringComparison.OrdinalIgnoreCase) && SymbolEqualityComparer.Default.Equals(p.Type, mParam.Type)))
                            continue;

                        parameters.Add((mParam.Name, mParam.Type));
                    }
                }
            }
        }

        return parameters.Select(p => p.Type).ToList();
    }

    private static bool HasFieldInitializer(IFieldSymbol field)
    {
        foreach (var syntaxRef in field.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is VariableDeclaratorSyntax declarator)
            {
                if (declarator.Initializer != null)
                    return true;
            }
        }
        return false;
    }

    private static void Execute(
        SourceProductionContext spc,
        Compilation compilation,
        ImmutableArray<List<ServiceInfo>> classesGroups,
        ImmutableArray<IMethodSymbol> methods)
    {
        var services = classesGroups.SelectMany(g => g).ToList();
        if (services.Count == 0 && methods.Length == 0)
            return;

        var assemblyName = compilation.AssemblyName ?? "App";
        var safeName = new string(assemblyName.Where(char.IsLetterOrDigit).ToArray());
        var resolverName = $"{safeName}Resolver";

        // Group services by implementation class so property is only declared once
        var byClass = services.GroupBy(s => s.ClassSymbol, SymbolEqualityComparer.Default).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine($"namespace {assemblyName};");
        sb.AppendLine();
        sb.AppendLine($"public sealed class {resolverName}(global::System.IServiceProvider sp) : global::HaselCommon.DependencyInjection.IServiceResolver, global::System.IDisposable, global::System.IAsyncDisposable");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly global::System.Collections.Generic.List<object> _disposables = [];");
        sb.AppendLine("    private readonly global::System.Threading.Lock _disposablesLock = new();");
        sb.AppendLine();
        sb.AppendLine("    private T TrackDisposable<T>(T instance)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (instance is global::System.IDisposable or global::System.IAsyncDisposable)");
        sb.AppendLine("        {");
        sb.AppendLine("            using (_disposablesLock.EnterScope())");
        sb.AppendLine("            {");
        sb.AppendLine("                if (!_disposables.Contains(instance))");
        sb.AppendLine("                    _disposables.Add(instance);");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        return instance;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate properties
        foreach (var group in byClass)
        {
            var first = group.First();
            var classSymbol = (INamedTypeSymbol)group.Key!;
            var fullClassTypeName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var propName = classSymbol.Name;

            string instExpr;
            if (first.FactoryMethod != null)
            {
                instExpr = $"{fullClassTypeName}.{first.FactoryMethod}(sp)";
            }
            else
            {
                var args = first.ConstructorParameters.Select(p =>
                {
                    if (p.ToDisplayString() == "System.IServiceProvider")
                        return "sp";

                    return $"sp.GetRequiredService<{p.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>()";
                });
                instExpr = $"new {fullClassTypeName}({string.Join(", ", args)})";
            }

            if (first.IsSingleton)
            {
                sb.AppendLine($"    private {fullClassTypeName} {propName}");
                sb.AppendLine("    {");
                sb.AppendLine($"        get => field ??= TrackDisposable({instExpr});");
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine($"    private {fullClassTypeName} {propName} => {instExpr};");
            }
            sb.AppendLine();
        }

        // Generate Resolve(Type serviceType)
        sb.AppendLine("    public object? Resolve(global::System.Type serviceType)");
        sb.AppendLine("    {");

        foreach (var group in byClass)
        {
            var first = group.First();
            var classSymbol = (INamedTypeSymbol)group.Key!;
            var fullClassTypeName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var propName = classSymbol.Name;

            // Direct implementation type check
            sb.AppendLine($"        if (serviceType == typeof({fullClassTypeName}))");
            sb.AppendLine($"            return {propName};");

            // Service type checks (if different and not DuplicateStrategy.Append)
            foreach (var reg in group)
            {
                var serviceTypeName = reg.ServiceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (serviceTypeName != fullClassTypeName && reg.DuplicateStrategy != 2 /* Append */)
                {
                    sb.AppendLine($"        if (serviceType == typeof({serviceTypeName}))");
                    sb.AppendLine($"            return {propName};");
                }
            }
        }

        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate ResolveAll(Type serviceType)
        sb.AppendLine("    public global::System.Collections.Generic.IEnumerable<object> ResolveAll(global::System.Type serviceType)");
        sb.AppendLine("    {");

        // Group by requested service type
        var byServiceType = services.GroupBy(s => s.ServiceType, SymbolEqualityComparer.Default);
        foreach (var group in byServiceType)
        {
            var serviceTypeName = group.Key!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            sb.AppendLine($"        if (serviceType == typeof({serviceTypeName}))");
            sb.AppendLine("        {");
            foreach (var reg in group)
            {
                sb.AppendLine($"            yield return {reg.ClassSymbol.Name};");
            }
            sb.AppendLine("            yield break;");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate IsService
        sb.AppendLine("    public bool IsService(Type serviceType)");
        sb.AppendLine("    {");
        foreach (var group in byClass)
        {
            var first = group.First();
            var classSymbol = (INamedTypeSymbol)group.Key!;
            var fullClassTypeName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var propName = classSymbol.Name;

            // Direct implementation type check
            sb.AppendLine($"        if (serviceType == typeof({fullClassTypeName}))");
            sb.AppendLine($"            return true;");

            // Service type checks (if different and not DuplicateStrategy.Append)
            foreach (var reg in group)
            {
                var serviceTypeName = reg.ServiceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (serviceTypeName != fullClassTypeName && reg.DuplicateStrategy != 2 /* Append */)
                {
                    sb.AppendLine($"        if (serviceType == typeof({serviceTypeName}))");
                    sb.AppendLine($"            return true;");
                }
            }
        }
        sb.AppendLine("        return false;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate Dispose and DisposeAsync
        sb.AppendLine("    public async global::System.Threading.Tasks.ValueTask DisposeAsync()");
        sb.AppendLine("    {");
        sb.AppendLine("        global::System.Collections.Generic.List<object> disposables;");
        sb.AppendLine();
        sb.AppendLine("        using (_disposablesLock.EnterScope())");
        sb.AppendLine("            disposables = [.. _disposables];");
        sb.AppendLine();
        sb.AppendLine("        for (var i = disposables.Count - 1; i >= 0; i--)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (disposables[i] is global::System.IAsyncDisposable asyncDisposable)");
        sb.AppendLine("                await asyncDisposable.DisposeAsync().ConfigureAwait(false);");
        sb.AppendLine("            else if (disposables[i] is global::System.IDisposable disposable)");
        sb.AppendLine("                disposable.Dispose();");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public void Dispose()");
        sb.AppendLine("    {");
        sb.AppendLine("        DisposeAsync().AsTask().GetAwaiter().GetResult();");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Generate ServiceCollectionExtensions
        sb.AppendLine($"public static class {safeName}ServiceCollectionExtensions");
        sb.AppendLine("{");
        sb.AppendLine($"    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection Add{safeName}(");
        sb.AppendLine("        this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
        sb.AppendLine("    {");
        sb.AppendLine($"        services.AddResolver(sp => new {resolverName}(sp));");

        foreach (var method in methods)
        {
            var containingType = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            sb.AppendLine($"        {containingType}.{method.Name}(services);");
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        spc.AddSource($"{resolverName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
