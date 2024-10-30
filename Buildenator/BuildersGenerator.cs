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
using System.Collections.Immutable;
using System.Threading;
using Buildenator.Diagnostics;

[assembly: InternalsVisibleTo("Buildenator.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Buildenator;

/// <inheritdoc />
[Generator]
public class BuildersGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        // Debugger.Launch();
#endif

        var nullableOptions = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Options.NullableContextOptions);
        var syntaxTrees = context.CompilationProvider
            .SelectMany(static (compilation, _) => compilation.SyntaxTrees);

        var symbolsAndAttributes = syntaxTrees
            .Combine(context.CompilationProvider)
            .Select(SelectSyntaxTreeAndSemanticModel)
            .Select(static (tuple, ct) => (Root: tuple.SyntaxTree.GetRoot(ct), tuple.SemanticModel))
            .SelectMany(static (tuple, _) =>
                tuple.Root.DescendantNodesAndSelf().Select(node => (node, tuple.SemanticModel)))
            .Where(static tuple => tuple.node is ClassDeclarationSyntax)
            .Select(static (tuple, token) => tuple.SemanticModel.GetDeclaredSymbol(tuple.node, token))
            .Where(static symbol => symbol is INamedTypeSymbol)
            .Select(static (symbol, _) => (INamedTypeSymbol)symbol!)
            .Select(static (symbol, _) => (Symbol: symbol, Attributes: symbol.GetAttributes()))
            .Select(static (symbolAndAttributesTuple, _)
                => (symbolAndAttributesTuple.Symbol,
                    Attribute: symbolAndAttributesTuple.Attributes.SingleOrDefault(attributeData =>
                        attributeData.AttributeClass?.Name == nameof(MakeBuilderAttribute))))
            .Where(static tuple => tuple.Attribute is not null /* remember about the bang operator in the next line when removing this condition */)
            .Select(static (tuple, _) => (BuilderSymbol: tuple.Symbol, Attribute: new MakeBuilderAttributeInternal(tuple.Attribute!)));

        var classSymbols = symbolsAndAttributes
            .Where(tuple => !tuple.Attribute.TypeForBuilder.IsAbstract);

        var allAttributes = context.CompilationProvider
            .Select(static (c, _) => c.Assembly)
            .Select(static (assembly, _) => assembly.GetAttributes())
            .Select(static (attributes, _) =>
            (
                Mockings: attributes.Where(x =>
                    x.AttributeClass.HasNameOrBaseClassHas(nameof(MockingConfigurationAttribute))),
                Builders: attributes.Where(x =>
                    x.AttributeClass.HasNameOrBaseClassHas(nameof(BuildenatorConfigurationAttribute))),
                Fixtures: attributes.Where(x =>
                    x.AttributeClass.HasNameOrBaseClassHas(nameof(FixtureConfigurationAttribute)))
            ));

        var configurationBuilders = allAttributes
            .Select(static (attributes, _) =>
                (
                    Mocking: attributes.Mockings.FirstOrDefault(),
                    Builder: attributes.Builders.FirstOrDefault(),
                    Fixture: attributes.Fixtures.FirstOrDefault()
                )
            )
            .Select(static (assembly, _) =>
            {
                var globalFixtureProperties = assembly.Fixture?.ConstructorArguments;
                var mockingConfigurationBuilder = assembly.Mocking?.ConstructorArguments;
                var globalBuilderProperties = assembly.Builder?.ConstructorArguments;
                return (globalFixtureProperties, mockingConfigurationBuilder, globalBuilderProperties);
            });

        var generators = classSymbols
            .Combine(configurationBuilders)
            .Combine(nullableOptions)
            .Select(static (leftRightAndNullable, _) =>
            {
                var (builderNamedTypeSymbol, attribute) = leftRightAndNullable.Left.Left;
                var (globalFixtureProperties,
                    globalMockingConfiguration,
                    globalBuilderProperties) = leftRightAndNullable.Left.Right;
                var nullableOptions = leftRightAndNullable.Right;
                var builderAttributes = builderNamedTypeSymbol.GetAttributes();

                var mockingProperties = MockingProperties.CreateOrDefault(
                    globalMockingConfiguration,
                    GetMockingConfigurationOrDefault(builderAttributes));

                var fixtureProperties =
                    FixtureProperties.CreateOrDefault(
                        globalFixtureProperties,
                        GetLocalFixturePropertiesOrDefault(builderAttributes));

                var builderProperties =
                    BuilderProperties.Create(builderNamedTypeSymbol, attribute, globalBuilderProperties, nullableOptions.AnnotationsEnabled());

                return (fixtureProperties, mockingConfiguration: mockingProperties, builderProperties,
                    attribute.TypeForBuilder);
            })
            .Select(static (properties, _) =>
            {
                var (fixtureProperties, mockingProperties, builderProperties, typeForBuilder) = properties;
                return new BuilderSourceStringGenerator(builderProperties,
                    new EntityToBuild(typeForBuilder, mockingProperties, fixtureProperties,
                        builderProperties.NullableStrategy),
                    fixtureProperties,
                    mockingProperties);
            });

        context.RegisterSourceOutput(generators, static (productionContext, generator) =>
        {
            productionContext.AddCsSourceFile(generator.FileName,
                SourceText.From(generator.CreateBuilderCode(), Encoding.UTF8));
            foreach(var diagnostic in generator.Diagnostics)
            {
                productionContext.ReportDiagnostic(diagnostic);
            }
        });


        var abstractClassSymbols = symbolsAndAttributes
            .Where(tuple => tuple.Attribute.TypeForBuilder.IsAbstract)
            .Select((tuple, _) => (tuple.BuilderSymbol, tuple.Attribute.TypeForBuilder));

        context.RegisterSourceOutput(abstractClassSymbols, (productionContext, tuple)
            =>
        {
            productionContext.ReportDiagnostic(
                new BuildenatorDiagnostic(BuildenatorDiagnosticDescriptors.AbstractDiagnostic,
                    tuple.BuilderSymbol.Locations.First(),
                    tuple.TypeForBuilder.Name)
                );
        });
    }

    private static (SyntaxTree SyntaxTree, SemanticModel SemanticModel) SelectSyntaxTreeAndSemanticModel((SyntaxTree SyntaxTree, Compilation Compilation) tuple, CancellationToken _)
            => (tuple.SyntaxTree, SemanticModel: tuple.Compilation.GetSemanticModel(tuple.SyntaxTree));

    private static ImmutableArray<TypedConstant>? GetLocalFixturePropertiesOrDefault(ImmutableArray<AttributeData> attributeData)
    {
        var attribute = attributeData.SingleOrDefault(x => x.AttributeClass.HasNameOrBaseClassHas(nameof(FixtureConfigurationAttribute)));
        return attribute?.ConstructorArguments;
    }

    private static ImmutableArray<TypedConstant>? GetMockingConfigurationOrDefault(ImmutableArray<AttributeData> attributeData) =>
        attributeData
            .SingleOrDefault(x => x.AttributeClass.HasNameOrBaseClassHas(nameof(MockingConfigurationAttribute)))
            ?.ConstructorArguments;
}