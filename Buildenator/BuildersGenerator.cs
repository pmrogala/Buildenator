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
    public class BuildersGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var classSymbols = GetClassSymbols(context);

            foreach (var classSymbol in classSymbols)
            {
                var generator = new BuilderSourceStringGenerator(classSymbol.Builder, classSymbol.ClassToBuild);
                context.AddSource($"{classSymbol.Builder.Name}.cs", SourceText.From(generator.CreateBuilderCode(), Encoding.UTF8));
            }
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
            var attribute = classSyntax.AttributeLists.SelectMany(b => b.Attributes.Where(a => a.Name.ToString().Contains("MakeBuilder"))).FirstOrDefault();
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
    }
}