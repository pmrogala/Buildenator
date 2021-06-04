using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Buildenator
{
    [Generator]
    public class BuilderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            InjectAutoGenerateBuilderAttribute(context);

            var classSymbols = GetClassSymbols(context);

            foreach (var classSymbol in classSymbols)
            {
                context.AddSource($"{classSymbol.Builder.Name}.cs", SourceText.From(CreateBuilderCode(classSymbol.Builder, classSymbol.ClassToBuild), Encoding.UTF8));
            }
        }

        private static void InjectAutoGenerateBuilderAttribute(GeneratorExecutionContext context)
        {

            const string attributeText = @"
using System;

namespace Buildenator
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class MakeBuilderAttribute : Attribute
    {
        public MakeBuilderAttribute(Type typeForBuilder)
        {
            TypeForBuilder = typeForBuilder;
        }

        public Type TypeForBuilder { get; }
    }
}";

            context.AddSource("MakeBuilderAttribute.cs", SourceText.From(attributeText, Encoding.UTF8));

        }

        private static List<(INamedTypeSymbol Builder, INamedTypeSymbol ClassToBuild)> GetClassSymbols(GeneratorExecutionContext context)
        {
            // Debugger.Launch();
            var classSymbols = new List<(INamedTypeSymbol, INamedTypeSymbol)>();

            var compilation = context.Compilation;

            foreach (var syntaxTree in compilation.SyntaxTrees.Where(x => x.FilePath.Contains("Builders")))
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                classSymbols.AddRange(
                        syntaxTree.GetRoot().DescendantNodesAndSelf()
                        .OfType<ClassDeclarationSyntax>()
                        .Select(classSyntax => (compilation.GetTypeByMetadataName(GetFullNameFrom(classSyntax)), ExtractClassToBuildTypeInfo(semanticModel, classSyntax)))
                        .AreNotNull());
            }

            return classSymbols;
        }

        private static INamedTypeSymbol? ExtractClassToBuildTypeInfo(SemanticModel semanticModel, ClassDeclarationSyntax classSyntax)
        {
            var attribute = classSyntax.AttributeLists.SelectMany(b => b.Attributes.Where(a => a.Name.ToString().Contains("Buildenator.MakeBuilder"))).FirstOrDefault();
            if (attribute is null)
                return null;

            var id = attribute.ArgumentList?.Arguments.First().Expression.ChildNodes().OfType<IdentifierNameSyntax>().First();
            if (id is null)
                return null;

            return (INamedTypeSymbol?)semanticModel.GetTypeInfo(id).Type;
        }

        private static string GetFullNameFrom(ClassDeclarationSyntax s)
        {
            var @namespace = GetNamespaceFrom(s);
            return string.IsNullOrWhiteSpace(@namespace)
                            ? s.Identifier.ToString()
                            : $"{@namespace}.{s.Identifier}";
        }

        public static string GetNamespaceFrom(SyntaxNode s) =>
            s.Parent switch
            {
                NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Name.ToString(),
                null => string.Empty,
                _ => GetNamespaceFrom(s.Parent)
            };

        private static IEnumerable<IParameterSymbol> GetConstructorParameters(INamedTypeSymbol classSymbol)
        {
            // TODO: Strategies for multiple constructors
            var properties = classSymbol.Constructors.First().Parameters;
            var propertyNames = properties.Select(x => x.Name);

            var baseType = classSymbol.BaseType;

            return properties;
        }

        private static IEnumerable<IPropertySymbol> GetSetableProperties(ITypeSymbol typeSymbol)
        {
            var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(x => x.SetMethod is not null)
                .Where(x => x.CanBeReferencedByName).ToList();
            var propertyNames = properties.Select(x => x.Name);

            var baseType = typeSymbol.BaseType;

            while (baseType != null)
            {

                properties.AddRange(baseType.GetMembers().OfType<IPropertySymbol>()
                                            .Where(x => x.CanBeReferencedByName)
                                            .Where(x => x.SetMethod is not null)
                                            .Where(x => !propertyNames.Contains(x.Name)));

                baseType = baseType.BaseType;
            }

            return properties;
        }

        // TODO: Custom fixtures strategies
        // TODO: Nullable configurable
        private string CreateBuilderCode(INamedTypeSymbol builderSymbol, INamedTypeSymbol classSymbol)
             => $@"
using System;
using AutoFixture;
using {classSymbol.ContainingNamespace};
#nullable enable
namespace {builderSymbol.ContainingNamespace}
{{
    public partial class {builderSymbol.Name}
    {{
        private readonly Fixture _fixture = new Fixture();
{GenerateConstructor(classSymbol)}
{GeneratePropertiesCode(classSymbol)}
{GenerateBuildsCode(classSymbol)}
    }}
}}";

        private object GenerateConstructor(INamedTypeSymbol classSymbol)
        {
            var parameters = GetConstructorParameters(classSymbol);

            var output = new StringBuilder();
            output.AppendLine($@"
        public {classSymbol.Name}Builder()
        {{");
            foreach (var parameter in parameters)
            {
                output.AppendLine($@"            {parameter.UnderScoreName()} = _fixture.Create<{parameter.Type}>();");
            }
            output.AppendLine($@"
        }}");
            return output.ToString();
        }

        private static string GeneratePropertiesCode(INamedTypeSymbol classSymbol)
        {
            var parameters = GetConstructorParameters(classSymbol);

            var output = new StringBuilder();

            foreach (var parameter in parameters)
            {
                output.AppendLine($@"

        private {parameter.Type} {parameter.UnderScoreName()};

        public {classSymbol.Name}Builder With{parameter.PascalCaseName()}({parameter.Type} value)
        {{
            {parameter.UnderScoreName()} = value;
            return this;
        }}");

            }

            return output.ToString();
        }

        private static string GenerateBuildsCode(INamedTypeSymbol classSymbol)
        {
            var parameters = GetConstructorParameters(classSymbol);

            var output = new StringBuilder();

            output.AppendLine($@"        public {classSymbol.Name} Build()
        {{
            return new {classSymbol.Name}(");

            output.Append(string.Join(
                ",",
                parameters.Select(parameter => $@"
                {parameter.UnderScoreName()}")));

            output.AppendLine($@");
        }}
        
        public static {classSymbol.Name}Builder {classSymbol.Name} => new {classSymbol.Name}Builder();");

            return output.ToString();

        }
    }

    internal static class ParameterSymbolExtensions
    {
        public static string PascalCaseName(this IParameterSymbol symbol)
            => $"{symbol.Name.Substring(0, 1).ToUpperInvariant()}{symbol.Name.Substring(1)}";
        public static string UnderScoreName(this IParameterSymbol symbol)
            => $"_{symbol.Name.Substring(0, 1).ToLowerInvariant()}{symbol.Name.Substring(1)}";
    }
}