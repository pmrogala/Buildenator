using Buildenator.CodeAnalysis;
using Buildenator.Configuration.Contract;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Buildenator.Abstraction;
using Buildenator.Diagnostics;
using System.Text;

namespace Buildenator.Configuration;

internal sealed class EntityToBuild : IEntityToBuild
{
    public string Name { get; }
    public string FullName { get; }
    public string FullNameWithConstraints { get; }
    public Constructor? ConstructorToBuild { get; }
    public IReadOnlyList<ITypedSymbol> AllUniqueSettablePropertiesAndParameters => _uniqueTypedSymbols;
    public IReadOnlyList<ITypedSymbol> AllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch => _uniqueReadOnlyTypedSymbols;
    public string[] AdditionalNamespaces { get; }
    public IEnumerable<BuildenatorDiagnostic> Diagnostics => _diagnostics;
    public NullableStrategy NullableStrategy { get; }

    public EntityToBuild(
        INamedTypeSymbol typeForBuilder,
        IMockingProperties? mockingConfiguration,
        IFixtureProperties? fixtureConfiguration,
        NullableStrategy nullableStrategy,
        string? staticFactoryMethodName)
    {
        INamedTypeSymbol? entityToBuildSymbol;
        var additionalNamespaces = Enumerable.Empty<string>();
        if (typeForBuilder.IsGenericType)
        {
            entityToBuildSymbol = typeForBuilder.ConstructedFrom;
            additionalNamespaces = entityToBuildSymbol.TypeParameters.Where(a => a.ConstraintTypes.Any())
                .SelectMany(a => a.ConstraintTypes).Select(a => a.ContainingNamespace.ToDisplayString())
                .ToArray();
        }
        else
        {
            entityToBuildSymbol = typeForBuilder;
        }

        AdditionalNamespaces = additionalNamespaces.ToArray();
        Name = entityToBuildSymbol.Name;
        FullName = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
        FullNameWithConstraints = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance));

        ConstructorToBuild = Constructor.CreateConstructorOrDefault(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration, nullableStrategy, staticFactoryMethodName);
        (_properties, _uniqueReadOnlyTypedSymbols) = DividePropertiesBySetability(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration, nullableStrategy);
        _uniqueTypedSymbols = _properties;
        if (ConstructorToBuild is not null)
        {
            _properties = _properties.Where(x => !ConstructorToBuild.ContainsParameter(x.SymbolName)).ToList();
            _uniqueTypedSymbols = [.. _properties, .. ConstructorToBuild.Parameters];
            _uniqueReadOnlyTypedSymbols = _uniqueReadOnlyTypedSymbols.Where(x => !ConstructorToBuild.ContainsParameter(x.SymbolName)).ToList();
        }

        NullableStrategy = nullableStrategy;
    }

    public string GenerateBuildsCode(bool shouldGenerateMethodsForUnreachableProperties)
    {
        if (ConstructorToBuild is null)
            return "";

        var disableWarning = NullableStrategy == NullableStrategy.Enabled
            ? "#pragma warning disable CS8604\n"
            : string.Empty;
        var restoreWarning = NullableStrategy == NullableStrategy.Enabled
            ? "#pragma warning restore CS8604\n"
            : string.Empty;

        return $@"{disableWarning}        public {FullName} {DefaultConstants.BuildMethodName}()
        {{
            {GenerateLazyBuildEntityString(shouldGenerateMethodsForUnreachableProperties, ConstructorToBuild.Parameters)}
        }}
{restoreWarning}
";
    }

    private string GenerateLazyBuildEntityString(bool shouldGenerateMethodsForUnreachableProperties, IEnumerable<TypedSymbol> parameters)
    {
        var propertiesAssignment = _properties.Select(property => $"{property.SymbolName} = {GeneratePropertyValue(property)}").ComaJoin();
        var onlyConstructorString = string.Empty;
        if (ConstructorToBuild is StaticConstructor staticConstructor)
        {
            onlyConstructorString = @$"var result = {FullName}.{staticConstructor.Name}({parameters.Select(symbol => GenerateConstructorParameterValue(symbol)).ComaJoin()});
";
        }
        else
        {
            onlyConstructorString = @$"var result = new {FullName}({parameters.Select(symbol => GenerateConstructorParameterValue(symbol)).ComaJoin()})
            {{
{(string.IsNullOrEmpty(propertiesAssignment) ? string.Empty : $"                {propertiesAssignment}")}
            }};
";
        }

        return onlyConstructorString
            + $@"{(shouldGenerateMethodsForUnreachableProperties ? GenerateUnreachableProperties() : "")}
            {DefaultConstants.PostBuildMethodName}(result);
            return result;";

        string GenerateUnreachableProperties()
        {
            var output = new StringBuilder();
            output.AppendLine($"var t = typeof({FullName});");
            foreach (var a in AllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch)
            {
                // Always use nullable operators for reflection chain to avoid exceptions on get-only properties
                output.Append($"            t.GetProperty(\"{a.SymbolName}\")")
                    .Append("?")
                    .Append(".DeclaringType")
                    .Append("?")
                    .Append($".GetProperty(\"{a.SymbolName}\")")
                    .Append("?")
                    .Append(".SetMethod")
                    .Append("?")
                    .AppendLine($".Invoke(result, new object[] {{ {GeneratePropertyValue(a)} }});");
            }
            return output.ToString();
        }

        string GeneratePropertyValue(ITypedSymbol property)
        {
            // Check if this is a collection property with items to add
            if (CollectionMethodDetector.IsCollectionProperty(property, property.TypeSymbol))
            {
                var fieldName = $"_{property.SymbolName}ToAdd";
                return $"{fieldName}.Count > 0 ? {fieldName} : {property.GenerateLazyFieldValueReturn()}";
            }
            
            return property.GenerateLazyFieldValueReturn();
        }

        string GenerateConstructorParameterValue(ITypedSymbol parameter)
        {
            // Check if this is a collection parameter with items to add
            if (CollectionMethodDetector.IsCollectionProperty(parameter, parameter.TypeSymbol))
            {
                var fieldName = $"_{parameter.SymbolName}ToAdd";
                return $"{fieldName}.Count > 0 ? {fieldName} : {parameter.GenerateLazyFieldValueReturn()}";
            }
            
            return parameter.GenerateLazyFieldValueReturn();
        }
    }

    private static (TypedSymbol[] Settable, TypedSymbol[] ReadOnly) DividePropertiesBySetability(
        INamedTypeSymbol entityToBuildSymbol, IMockingProperties? mockingConfiguration,
        IFixtureProperties? fixtureConfiguration, NullableStrategy nullableStrategy)
    {
        var (settable, readOnly) = entityToBuildSymbol.DividePublicPropertiesBySetability();
        return (
            settable.Select(a => new TypedSymbol(a, mockingConfiguration, fixtureConfiguration, nullableStrategy)).ToArray(),
            readOnly.Select(a => new TypedSymbol(a, mockingConfiguration, fixtureConfiguration, nullableStrategy)).ToArray());
    }

    public string GenerateDefaultBuildsCode()
    {
        if (ConstructorToBuild is null)
            return "";

        var moqInit = ConstructorToBuild.Parameters
            .Concat(_properties)
            .Where(symbol => symbol.IsMockable())
            .Select(s => $@"            {s.GenerateFieldInitialization()}")
            .Aggregate(new StringBuilder(), (builder, s) => builder.AppendLine(s))
            .ToString();

        var methodParameters = ConstructorToBuild.Parameters
            .Concat(_properties)
            .Select(s =>
            {
                var fieldType = s.GenerateFieldType();
                return $"{fieldType} {s.UnderScoreName} = default({fieldType})";
            }).ComaJoin();
        var disableWarning = NullableStrategy == NullableStrategy.Enabled
            ? "#pragma warning disable CS8625\n"
            : string.Empty;
        var restoreWarning = NullableStrategy == NullableStrategy.Enabled
            ? "#pragma warning restore CS8625\n"
            : string.Empty;

        return $@"{disableWarning}        public static {FullName} BuildDefault({methodParameters})
        {{
            {moqInit}
            {GenerateDefaultBuildEntityString(ConstructorToBuild.Parameters)}
        }}
{restoreWarning}";
    }

    private string GenerateDefaultBuildEntityString(IEnumerable<TypedSymbol> parameters)
    {
        if (ConstructorToBuild is StaticConstructor staticConstructor)
        {
            return @$"return {FullName}.{staticConstructor.Name}({parameters.Select(a => a.GenerateFieldValueReturn()).ComaJoin()});";
        }
        else
        {
            var propertiesAssignment = _properties.Select(property => $"{property.SymbolName} = {property.GenerateFieldValueReturn()}").ComaJoin();
            return @$"return new {FullName}({parameters.Select(a => a.GenerateFieldValueReturn()).ComaJoin()})
            {{
{(string.IsNullOrEmpty(propertiesAssignment) ? string.Empty : $"                {propertiesAssignment}")}
            }};";
        }
    }

    private readonly IReadOnlyList<TypedSymbol> _uniqueReadOnlyTypedSymbols;
    private readonly IReadOnlyList<TypedSymbol> _uniqueTypedSymbols;
    private readonly IReadOnlyList<TypedSymbol> _properties;
    private readonly List<BuildenatorDiagnostic> _diagnostics = [];

    internal abstract class Constructor(IReadOnlyDictionary<string, TypedSymbol> constructorParameters)
    {
        public static Constructor? CreateConstructorOrDefault(
            INamedTypeSymbol entityToBuildSymbol,
            IMockingProperties? mockingConfiguration,
            IFixtureProperties? fixtureConfiguration,
            NullableStrategy nullableStrategy,
            string? staticFactoryMethodName)
        {
            IEnumerable<IMethodSymbol> constructors;
            if (staticFactoryMethodName is null)
            {
                constructors = entityToBuildSymbol.Constructors.Select(a => a);
            }
            else
            {
                constructors = entityToBuildSymbol.GetMembers(staticFactoryMethodName).OfType<IMethodSymbol>().Where(a => a.IsStatic);
            }

            var onlyPublicConstructors = constructors
                .Where(m => m.DeclaredAccessibility == Accessibility.Public || m.DeclaredAccessibility == Accessibility.Internal)
                .ToList();

            if (onlyPublicConstructors.Count == 0)
                return default;

            var selectedConstructor = onlyPublicConstructors
                            .OrderByDescending(x => x.Parameters.Length)
                            .First();
            var parameters = selectedConstructor
                .Parameters
                .ToDictionary(x => x.PascalCaseName(), s => new TypedSymbol(s, mockingConfiguration, fixtureConfiguration, nullableStrategy));

            return staticFactoryMethodName is null
                ? new ObjectConstructor(parameters)
                : new StaticConstructor(parameters, selectedConstructor.Name);
        }

        public IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; } = constructorParameters;

        public bool ContainsParameter(string parameterName) => ConstructorParameters.ContainsKey(parameterName);
        public IEnumerable<TypedSymbol> Parameters => ConstructorParameters.Values;
    }

    internal sealed class ObjectConstructor(IReadOnlyDictionary<string, TypedSymbol> constructorParameters) : Constructor(constructorParameters) { }

    internal sealed class StaticConstructor(IReadOnlyDictionary<string, TypedSymbol> constructorParameters, string name)
        : Constructor(constructorParameters)
    {
        public string Name { get; } = name;
    }
}