using Buildenator.Abstraction;
using Buildenator.Configuration;
using Buildenator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Buildenator.Extensions;
using System;

[assembly: InternalsVisibleTo("Buildenator.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Buildenator
{
    /// <inheritdoc />
    [Generator]
    public class BuildersGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            // Debugger.Launch();
#endif
            var syntaxTrees = context.CompilationProvider
                .SelectMany(static (c, _) => c.SyntaxTrees);

            var symbolsAndAttributes = syntaxTrees
                .Combine(context.CompilationProvider)
                .Select(static (tuple, _) => (compilation: tuple.Left, SemanticModel: tuple.Right.GetSemanticModel(tuple.Left)))
                .Select(static (tuple, ct) => (Root: tuple.compilation.GetRoot(ct), tuple.SemanticModel))
                .SelectMany(static (tuple, _) => tuple.Root.DescendantNodesAndSelf().Select(node => (node, tuple.SemanticModel)))
                .Where(static tuple => tuple.node is ClassDeclarationSyntax)
                .Select(static (tuple, token) => tuple.SemanticModel.GetDeclaredSymbol(tuple.node, token))
                .Where(static symbol => symbol is INamedTypeSymbol)
                .Select(static (symbol, _) => (INamedTypeSymbol)symbol!)
                .Select(static (symbol, _) => (symbol, attributes: symbol!.GetAttributes()))
                .Select(static (symbol, _) 
                    => (symbol.symbol, attribute: symbol.attributes.SingleOrDefault(x => x.AttributeClass?.Name == nameof(MakeBuilderAttribute))))
                .Where(static tuple => tuple.attribute is not null)
                .Select(static (tuple, _) => (tuple.symbol, new MakeBuilderAttributeInternal(tuple.attribute!)));

            var classSymbols = symbolsAndAttributes
                .Where(tuple => !tuple.Item2.TypeForBuilder.IsAbstract);


            var configurationBuilders = context.CompilationProvider
                .Select(static (c, _) => c.Assembly)
                .Select(static (assembly, _) => assembly.GetAttributes())
                .Select(static (assembly, _) =>
                {
                    var fixtureConfigurationBuilder = new FixturePropertiesBuilder(assembly);
                    var mockingConfigurationBuilder = new MockingPropertiesBuilder(assembly);
                    var builderPropertiesBuilder = new BuilderPropertiesBuilder(assembly);
                    return (fixtureConfigurationBuilder, mockingConfigurationBuilder, builderPropertiesBuilder);
                });

            var generators = classSymbols.Combine(configurationBuilders)
                .Select(static (leftRight, _) =>
                {
                    var (builder, attribute) = leftRight.Left;
                    var (fixtureConfigurationBuilder, mockingConfigurationBuilder, builderPropertiesBuilder) =
                        leftRight.Right;
                    var mockingConfiguration = mockingConfigurationBuilder.Build(builder);
                    var fixtureConfiguration = fixtureConfigurationBuilder.Build(builder);
                    var builderProperties = builderPropertiesBuilder.Build(builder, attribute);

                    return (fixtureConfiguration, mockingConfiguration, builderProperties, attribute.TypeForBuilder);
                })
                .Select(static (properties, _) =>
                {
                    var (fixtureConfiguration, mockingConfiguration, builderProperties, typeForBuilder) = properties;
                    return new BuilderSourceStringGenerator(builderProperties,
                         new EntityToBuild(typeForBuilder, mockingConfiguration, fixtureConfiguration),
                         fixtureConfiguration,
                         mockingConfiguration);
                });

            context.RegisterSourceOutput(generators, static (productionContext, generator) =>
            {
                productionContext.AddCsSourceFile(generator.FileName,
                    SourceText.From(generator.CreateBuilderCode(), Encoding.UTF8));
            });

            
            var abstractClassSymbols = symbolsAndAttributes
                .Where(tuple => tuple.Item2.TypeForBuilder.IsAbstract)
                .Select((tuple, _) => tuple.symbol);
            
            context.RegisterSourceOutput(abstractClassSymbols, (productionContext, classSymbol) 
                =>
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(AbstractDiagnostic, classSymbol.Locations.First(),
                    classSymbol.Name));
            });

        }

        private static readonly DiagnosticDescriptor AbstractDiagnostic = new("BDN001", "Cannot generate a builder for an abstract class", "Cannot generate a builder for the {0} abstract class", "Buildenator", DiagnosticSeverity.Error, true);

    }
}
