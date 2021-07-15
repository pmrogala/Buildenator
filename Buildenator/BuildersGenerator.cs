using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Buildenator
{
    [Generator()]
    public class BuildersGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Debugger.Launch();
            var classSymbols = GetBuilderSymbolAndItsAttribute(context);

            var compilation = context.Compilation;
            var assembly = compilation.Assembly;
            var fixtureConfigurationBuilder = new FixturePropertiesBuilder(assembly);
            var mockingConfigurationBuilder = new MockingPropertiesBuilder(assembly);

            foreach (var (builder, attribute) in classSymbols)
            {
                var generator = new BuilderSourceStringGenerator(
                    new BuilderProperties(builder, attribute),
                    new EntityToBuildProperties(attribute),
                    fixtureConfigurationBuilder.Build(builder),
                    mockingConfigurationBuilder.Build(builder));

                context.AddSource($"{builder.Name}.cs", SourceText.From(generator.CreateBuilderCode(), Encoding.UTF8));

                if (context.CancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private static IReadOnlyCollection<(INamedTypeSymbol Builder, MakeBuilderAttributeInternal Attribute)> 
            GetBuilderSymbolAndItsAttribute(GeneratorExecutionContext context)
        {
            var result = new List<(INamedTypeSymbol, MakeBuilderAttributeInternal)>();

            var compilation = context.Compilation;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                foreach (var classSyntax in syntaxTree.GetRoot(context.CancellationToken).DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken);
                    if (classSymbol is not INamedTypeSymbol namedClassSymbol)
                        continue;

                    var attribute = namedClassSymbol.GetAttributes().Where(x => x.AttributeClass?.Name == nameof(MakeBuilderAttribute)).SingleOrDefault();
                    if (attribute is null)
                        continue;

                    result.Add((namedClassSymbol, CreateMakeBuilderAttributeInternal(attribute)));
                }
            }

            return result;
        }

        private static MakeBuilderAttributeInternal CreateMakeBuilderAttributeInternal(AttributeData attribute)
            => new MakeBuilderAttributeInternal(
                (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!,
                (string)attribute.ConstructorArguments[1].Value!);
    }
}