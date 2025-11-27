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
using System.Collections.Generic;

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
        var symbolsAndAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(MakeBuilderAttribute).FullName,
            predicate: static (sx, _) => sx is ClassDeclarationSyntax,
            transform: static (ctx, _) =>
            {
                var attributes = ctx.TargetSymbol.GetAttributes();
                return (
                BuilderSymbol: (INamedTypeSymbol)ctx.TargetSymbol,
                BuilderAttribute: new MakeBuilderAttributeInternal(
                    attributes.Single(
                        attributeData => attributeData.AttributeClass?.Name == nameof(MakeBuilderAttribute))),
                MockingAttribute: GetMockingConfigurationOrDefault(attributes),
                FixtureAttribute: GetLocalFixturePropertiesOrDefault(attributes)
                        );
            }
        );

        var nullableOptions = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Options.NullableContextOptions);

        var classSymbols = symbolsAndAttributes
            .Where(tuple => !tuple.BuilderAttribute.TypeForBuilder.IsAbstract);

        var configurationBuilders = context.CompilationProvider
            .Select(static (c, _) => c.Assembly)
            .Select(static (assembly, _) => assembly.GetAttributes())
            .Select(static (attributes, _) =>
            (
                Mocking: attributes.Where(x =>
                    x.AttributeClass.HasNameOrBaseClassHas(nameof(MockingConfigurationAttribute)))
                .FirstOrDefault(),
                Builder: attributes.Where(x =>
                    x.AttributeClass.HasNameOrBaseClassHas(nameof(BuildenatorConfigurationAttribute)))
                .FirstOrDefault(),
                Fixture: attributes.Where(x =>
                    x.AttributeClass.HasNameOrBaseClassHas(nameof(FixtureConfigurationAttribute)))
                .FirstOrDefault()
            ))
            .Select(static (assembly, _) =>
            {
                var globalFixtureProperties = assembly.Fixture?.ConstructorArguments;
                var mockingConfigurationBuilder = assembly.Mocking?.ConstructorArguments;
                var globalBuilderProperties = assembly.Builder?.ConstructorArguments;

                return (globalFixtureProperties, mockingConfigurationBuilder, globalBuilderProperties);
            });

        // Collect all builders and their entity types to create a mapping
        // This allows us to find child builders for properties when useChildBuilders is enabled
        var allBuilderMappings = classSymbols
            .Collect()
            .Select(static (builders, _) =>
            {
                var mapping = new Dictionary<string, string>();
                foreach (var builder in builders)
                {
                    var entityFullName = builder.BuilderAttribute.TypeForBuilder.ToDisplayString(
                        new SymbolDisplayFormat(
                            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
                    var builderFullName = builder.BuilderSymbol.ToDisplayString(
                        new SymbolDisplayFormat(
                            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
                    // If there are multiple builders for the same entity, the last one wins
                    mapping[entityFullName] = builderFullName;
                }
                return mapping.ToImmutableDictionary();
            });

        var generators = classSymbols
            .Combine(configurationBuilders)
            .Combine(nullableOptions)
            .Combine(allBuilderMappings)
            .Select(static (combined, _) =>
            {
                var (builderNamedTypeSymbol, builderAttribute, mockingAttribute, fixtureAttribute) = combined.Left.Left.Left;
                var (globalFixtureProperties,
                    globalMockingConfiguration,
                    globalBuilderProperties) = combined.Left.Left.Right;
                var nullableOptions = combined.Left.Right;
                var builderMappings = combined.Right;

                var mockingProperties = MockingProperties.CreateOrDefault(globalMockingConfiguration, mockingAttribute);

                var fixtureProperties =
                    FixtureProperties.CreateOrDefault(globalFixtureProperties, fixtureAttribute);

                var builderProperties =
                    BuilderProperties.Create(builderNamedTypeSymbol, builderAttribute, globalBuilderProperties, nullableOptions.AnnotationsEnabled());

                return (fixtureProperties, mockingConfiguration: mockingProperties, builderProperties,
                    builderAttribute.TypeForBuilder, builderMappings);
            })
            .Select(static (properties, _) =>
            {
                var (fixtureProperties, mockingProperties, builderProperties, typeForBuilder, builderMappings) = properties;
                return new BuilderSourceStringGenerator(builderProperties,
                    new EntityToBuild(
                        typeForBuilder,
                        mockingProperties,
                        fixtureProperties,
                        builderProperties.NullableStrategy,
                        builderProperties.StaticFactoryMethodName,
                        builderProperties.DefaultValueNames),
                    fixtureProperties,
                    mockingProperties,
                    builderMappings);
            });

        context.RegisterSourceOutput(generators, static (productionContext, generator) =>
        {
            productionContext.AddCsSourceFile(generator.FileName,
                SourceText.From(generator.CreateBuilderCode(), Encoding.UTF8));
            foreach (var diagnostic in generator.Diagnostics)
            {
                productionContext.ReportDiagnostic(diagnostic);
            }
        });


        var abstractClassSymbols = symbolsAndAttributes
            .Where(tuple => tuple.BuilderAttribute.TypeForBuilder.IsAbstract)
            .Select((tuple, _) => (tuple.BuilderSymbol, tuple.BuilderAttribute.TypeForBuilder));

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