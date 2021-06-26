using Buildenator.Abstraction;
using Buildenator.Extensions;
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
            // Debugger.Launch();
            var classSymbols = GetClassSymbols(context);

            var compilation = context.Compilation;
            var assembly = compilation.Assembly;
            var fixtureConfigurationBuilder = new FixtureConfigurationBuilder(assembly);

            foreach (var classSymbol in classSymbols)
            {
                var generator = new BuilderSourceStringGenerator(
                    new BuilderProperties(classSymbol.Builder, classSymbol.Attribute),
                    new EntityToBuildProperties(classSymbol.ClassToBuild),
                    fixtureConfigurationBuilder.Build(classSymbol.Builder));
                context.AddSource($"{classSymbol.Builder.Name}.cs", SourceText.From(generator.CreateBuilderCode(), Encoding.UTF8));
            }
        }

        private static List<(INamedTypeSymbol Builder, INamedTypeSymbol ClassToBuild, AttributeData Attribute)> 
            GetClassSymbols(GeneratorExecutionContext context)
        {
            var classSymbols = new List<(INamedTypeSymbol, INamedTypeSymbol, AttributeData)>();

            var compilation = context.Compilation;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                classSymbols.AddRange(
                        syntaxTree.GetRoot().DescendantNodesAndSelf()
                        .OfType<ClassDeclarationSyntax>()
                        .Select(x => semanticModel.GetDeclaredSymbol(x))
                        .OfType<INamedTypeSymbol>()
                        .Select(classSymbol => (classSymbol, classSymbol.GetAttributes().Where(x => x.AttributeClass?.Name == nameof(MakeBuilderAttribute)).SingleOrDefault()))
                        .Where(x => x.Item2 != null)
                        .Select(tuple => (tuple.classSymbol, ExtractClassToBuildTypeInfo(tuple.Item2), tuple.Item2)));
            }

            return classSymbols;
        }

        private static INamedTypeSymbol ExtractClassToBuildTypeInfo(AttributeData attribute)
        {
            return (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!;
        }
    }
}