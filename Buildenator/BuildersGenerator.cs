using Buildenator.Abstraction;
using Buildenator.Configuration;
using Buildenator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
#if DEBUG
#endif
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Buildenator.Extensions;

[assembly: InternalsVisibleTo("Buildenator.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Buildenator
{
	/// <inheritdoc />
	[Generator]
    public class BuildersGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            // Debugger.Launch();
#endif
            var classSymbols = GetSortedBuilderSymbolAndItsAttribute(context);

            var compilation = context.Compilation;
            var assembly = compilation.Assembly;
            var fixtureConfigurationBuilder = new FixturePropertiesBuilder(assembly);
            var mockingConfigurationBuilder = new MockingPropertiesBuilder(assembly);
            var builderPropertiesBuilder = new BuilderPropertiesBuilder(assembly);

            foreach (var (builder, attribute) in classSymbols)
            {
                var mockingConfiguration = mockingConfigurationBuilder.Build(builder);
                var fixtureConfiguration = fixtureConfigurationBuilder.Build(builder);
                var generator = new BuilderSourceStringGenerator(
                builderPropertiesBuilder.Build(builder, attribute),
                new EntityToBuild(attribute.TypeForBuilder, mockingConfiguration, fixtureConfiguration),
                    fixtureConfiguration,
                    mockingConfiguration);

                context.AddCsSourceFile(builder.Name, SourceText.From(generator.CreateBuilderCode(), Encoding.UTF8));

                if (context.CancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private static IReadOnlyCollection<(INamedTypeSymbol Builder, MakeBuilderAttributeInternal Attribute)> 
            GetSortedBuilderSymbolAndItsAttribute(GeneratorExecutionContext context)
        {
            var result = new List<(INamedTypeSymbol Builder, MakeBuilderAttributeInternal)>();

            var compilation = context.Compilation;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                foreach (var classSyntax in syntaxTree.GetRoot(context.CancellationToken).DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken);
                    if (classSymbol is not { } namedClassSymbol)
                        continue;

                    var attribute = namedClassSymbol.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Name == nameof(MakeBuilderAttribute));
                    if (attribute is null)
                        continue;

                    var makeBuilderAttribute = new MakeBuilderAttributeInternal(attribute);

                    if (makeBuilderAttribute.TypeForBuilder.IsAbstract)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(AbstractDiagnostic, classSymbol.Locations.First(), classSymbol.Name));
                        continue;
                    }

                    result.Add((namedClassSymbol, makeBuilderAttribute));
                }
            }

            result.MakeDeterministicOrderByName();
            return result;
        }

        private static readonly DiagnosticDescriptor AbstractDiagnostic = new ("BDN001", "Cannot generate a builder for an abstract class", "Cannot generate a builder for the {0} abstract class", "Buildenator", DiagnosticSeverity.Error, true);
    }
}
