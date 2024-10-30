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
    public IReadOnlyList<TypedSymbol> SettableProperties { get; }
    public IReadOnlyList<TypedSymbol> ReadOnlyProperties { get; }
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

        AdditionalNamespaces = additionalNamespaces.Concat([entityToBuildSymbol.ContainingNamespace.ToDisplayString()]).ToArray();
        Name = entityToBuildSymbol.Name;
        FullName = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
        FullNameWithConstraints = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance));

        ConstructorToBuild = Constructor.CreateConstructorOrDefault(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration, nullableStrategy, staticFactoryMethodName);
        (SettableProperties, ReadOnlyProperties) = DividePropertiesBySetability(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration, nullableStrategy);
        NullableStrategy = nullableStrategy;
    }

    public IReadOnlyList<ITypedSymbol> GetAllUniqueSettablePropertiesAndParameters()
    {
        if (ConstructorToBuild is null)
        {
            return _uniqueTypedSymbols ??= SettableProperties;
        }

        return _uniqueTypedSymbols ??= SettableProperties
            .Where(x => !ConstructorToBuild.ContainsParameter(x.SymbolName))
            .Concat(ConstructorToBuild.Parameters).ToList();
    }

    public IReadOnlyList<ITypedSymbol> GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch()
    {
        if (ConstructorToBuild is null)
        {
            return _uniqueReadOnlyTypedSymbols ??= ReadOnlyProperties;
        }

        return _uniqueReadOnlyTypedSymbols ??= ReadOnlyProperties
            .Where(x => !ConstructorToBuild.ContainsParameter(x.SymbolName)).ToList();
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

    public string GenerateDefaultBuildEntityString(IEnumerable<ITypedSymbol> parameters, IEnumerable<ITypedSymbol> properties)
    {
        if (ConstructorToBuild is StaticConstructor staticConstructor)
        {
            return @$"return {FullName}.{staticConstructor.Name}({parameters.Select(a => a.GenerateFieldValueReturn()).ComaJoin()});";
        }
        else
        {
            var propertiesAssignment = properties.Select(property => $"{property.SymbolName} = {property.GenerateFieldValueReturn()}").ComaJoin();
            return @$"return new {FullName}({parameters.Select(a => a.GenerateFieldValueReturn()).ComaJoin()})
            {{
{(string.IsNullOrEmpty(propertiesAssignment) ? string.Empty : $"                {propertiesAssignment}")}
            }};";
        }
    }

    public (IReadOnlyList<ITypedSymbol> Parameters, IReadOnlyList<ITypedSymbol> Properties) GetParametersAndProperties()
    {
        IEnumerable<TypedSymbol> parameters = [];
        IEnumerable<TypedSymbol> properties = SettableProperties;
        if (ConstructorToBuild is not null)
        {
            parameters = ConstructorToBuild.Parameters;
            properties = properties.Where(x => !ConstructorToBuild.ContainsParameter(x.SymbolName));
        }

        return (parameters.ToList(), properties.ToList());
    }

    public string GenerateStaticBuildsCode()
    {
        if (ConstructorToBuild is null)
            return "";

        var (parameters, properties) = GetParametersAndProperties();
        var moqInit = parameters
            .Concat(properties)
            .Where(symbol => symbol.IsMockable())
            .Select(s => $@"            {s.GenerateFieldInitialization()}")
            .Aggregate(new StringBuilder(), (builder, s) => builder.AppendLine(s))
            .ToString();

        var methodParameters = parameters
            .Concat(properties)
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
            {GenerateDefaultBuildEntityString(parameters, properties)}
        }}
{restoreWarning}";
    }

    private IReadOnlyList<TypedSymbol>? _uniqueTypedSymbols;
    private IReadOnlyList<TypedSymbol>? _uniqueReadOnlyTypedSymbols;
    private readonly List<BuildenatorDiagnostic> _diagnostics = [];

    internal abstract class Constructor
    {
        public static Constructor? CreateConstructorOrDefault(
            INamedTypeSymbol entityToBuildSymbol,
            IMockingProperties? mockingConfiguration,
            IFixtureProperties? fixtureConfiguration,
            NullableStrategy nullableStrategy,
            string? staticFactoryMethodName)
        {
            IMethodSymbol[] constructors;
            if (staticFactoryMethodName is null)
            {
                constructors = entityToBuildSymbol.Constructors.Select(a => a).ToArray();
            }
            else
            {
                constructors = entityToBuildSymbol.GetMembers(staticFactoryMethodName).OfType<IMethodSymbol>().Where(a => a.IsStatic).ToArray();
            }

            var onlyPublicConstructors = constructors
                .Where(m => m.DeclaredAccessibility == Accessibility.Public || m.DeclaredAccessibility == Accessibility.Internal)
                .ToList();

            if (onlyPublicConstructors.Count == 0)
                return default;

            Dictionary<string, TypedSymbol> parameters = [];

            var selectedConstructor = onlyPublicConstructors
                            .OrderByDescending(x => x.Parameters.Length)
                            .First();
            parameters = selectedConstructor
                .Parameters
                .ToDictionary(x => x.PascalCaseName(), s => new TypedSymbol(s, mockingConfiguration, fixtureConfiguration, nullableStrategy));

            return staticFactoryMethodName is null
                ? new ObjectConstructor(parameters)
                : new StaticConstructor(parameters, selectedConstructor.Name);
        }

        protected Constructor(IReadOnlyDictionary<string, TypedSymbol> constructorParameters)
        {
            ConstructorParameters = constructorParameters;
        }

        public IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; }

        public bool ContainsParameter(string parameterName) => ConstructorParameters.ContainsKey(parameterName);
        public IEnumerable<TypedSymbol> Parameters => ConstructorParameters.Values;
    }

    internal sealed class ObjectConstructor : Constructor
    {
        internal ObjectConstructor(IReadOnlyDictionary<string, TypedSymbol> constructorParameters)
            : base(constructorParameters)
        {
        }
    }

    internal sealed class StaticConstructor : Constructor
    {
        internal StaticConstructor(IReadOnlyDictionary<string, TypedSymbol> constructorParameters, string name)
            : base(constructorParameters)
        {
            Name = name;
        }
        public string Name { get; }
    }
}