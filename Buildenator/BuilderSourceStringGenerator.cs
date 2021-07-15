using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Buildenator
{
    internal class BuilderSourceStringGenerator
    {
        private readonly BuilderProperties _builder;
        private readonly EntityToBuildProperties _entity;
        private readonly FixtureProperties? _fixtureConfiguration;
        private readonly MockingProperties? _mockingConfiguration;
        private const string SetupActionLiteral = "setupAction";
        private const string ValueLiteral = "value";
        private const string FixtureLiteral = "_fixture";

        public BuilderSourceStringGenerator(
            BuilderProperties builder,
            EntityToBuildProperties entity,
            FixtureProperties? fixtureConfiguration,
            MockingProperties? mockingConfiguration)
        {
            _builder = builder;
            _entity = entity;
            _fixtureConfiguration = fixtureConfiguration;
            _mockingConfiguration = mockingConfiguration;
        }

        public string CreateBuilderCode()
             => $@"
{GenerateNamespaces()}

namespace {_builder.ContainingNamespace}
{{
    public partial class {_builder.Name}
    {{
        {(_fixtureConfiguration is null ? string.Empty : $"private readonly {_fixtureConfiguration.Name} {FixtureLiteral} = new {_fixtureConfiguration.Name}()")};
{GenerateConstructor()}
{GeneratePropertiesCode()}
{GenerateBuildsCode()}
    }}
}}";

        private string GenerateNamespaces()
        {
            var list = new string[]
            {
                "System",
                _entity.ContainingNamespace
            }
                .Concat(_fixtureConfiguration?.AdditionalNamespaces ?? Enumerable.Empty<string>())
                .Concat(_mockingConfiguration?.AdditionalNamespaces ?? Enumerable.Empty<string>());

            if (_fixtureConfiguration is not null)
                list = list.Append(_fixtureConfiguration.Namespace);

            list = list.Distinct();

            var output = new StringBuilder();
            foreach (var @namespace in list)
            {
                output.Append("using ").Append(@namespace).AppendLine(";");
            }
            return output.ToString();
        }

        private string GenerateConstructor()
        {
            var parameters = _entity.GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();
            output.AppendLine($@"
        public {_builder.Name}()
        {{");
            foreach (var (parameter, type) in parameters)
            {
                output.AppendLine($@"            {parameter.UnderScoreName()} = {GenerateCreatingFieldInitialization(type)};");
            }
            output.AppendLine($@"
        }}");
            return output.ToString();
        }

        private string GenerateCreatingFieldInitialization(ITypeSymbol typeSymbol)
            => IsMockable(typeSymbol)
                ? string.Format(_mockingConfiguration!.FieldDeafultValueAssigmentFormat, typeSymbol.ToDisplayString())
                : IsFakeable(typeSymbol)
                    ? $"{FixtureLiteral}.Create<{typeSymbol}>()"
                    : $"default({typeSymbol})";

        private bool IsMockable(ITypeSymbol typeSymbol)
            => _mockingConfiguration switch
            {
                MockingProperties { Strategy: Abstraction.MockingInterfacesStrategy.All }
                    when typeSymbol.TypeKind == TypeKind.Interface => true,
                MockingProperties { Strategy: Abstraction.MockingInterfacesStrategy.WithoutGenericCollection }
                    when typeSymbol.TypeKind == TypeKind.Interface && typeSymbol.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => true,
                _ => false
            };

        private bool IsFakeable(ITypeSymbol typeSymbol)
            => _fixtureConfiguration switch
            {
                null => false,
                FixtureProperties { Strategy: Abstraction.FixtureInterfacesStrategy.None }
                    when typeSymbol.TypeKind == TypeKind.Interface => false,
                FixtureProperties { Strategy: Abstraction.FixtureInterfacesStrategy.OnlyGenericCollections }
                    when typeSymbol.TypeKind == TypeKind.Interface && typeSymbol.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => false,
                _ => true
            };

        private string GeneratePropertiesCode()
        {
            var properties = _entity.GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();

            foreach (var (property, type) in properties
                .Where(x => !_builder.Fields.TryGetValue(x.Symbol.UnderScoreName(), out var field) || field.Type.Name != x.Type.Name))
            {
                output.AppendLine($@"        private {GenerateFieldType(type)} {property.UnderScoreName()};");
            }

            foreach (var (property, type) in properties.Where(x => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x.Symbol), out var method)
                 || !(method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.Type.Name)))
            {
                output.AppendLine($@"

        public {_builder.Name} {GenerateMethodName(property, type)}
        {{
            {GenerateValueAssigment(property, type)};
            return this;
        }}");

            }

            return output.ToString();
        }

        private string GenerateValueAssigment(ISymbol symbol, ITypeSymbol typeSymbol)
            => IsMockable(typeSymbol)
                ? $"{SetupActionLiteral}({symbol.UnderScoreName()})"
                : $"{symbol.UnderScoreName()} = {ValueLiteral}";

        private string CreateMethodName(ISymbol property) => $"{_builder.BuildingMethodsPrefix}{property.PascalCaseName()}";

        private string GenerateMethodName(ISymbol symbol, ITypeSymbol type)
            => $"{CreateMethodName(symbol)}{(IsMockable(type) ? $"(Action<{CreateMockableFieldType(type)}> {SetupActionLiteral})" : $"({type} {ValueLiteral})")}";

        private object GenerateFieldType(ITypeSymbol type) => IsMockable(type) ? CreateMockableFieldType(type) : type.ToDisplayString();

        private string CreateMockableFieldType(ITypeSymbol type) => string.Format(_mockingConfiguration!.TypeDeclarationFormat, type.ToDisplayString());

        private string GenerateBuildsCode()
        {
            var parameters = _entity.ConstructorParameters;
            var properties = _entity.SettableProperties
                .Where(x => !parameters.ContainsKey(x.Name));

            return $@"        public {_entity.Name} Build()
        {{
            return new {_entity.Name}({string.Join(", ", parameters.Values.Select(parameter => GenerateFieldValueReturn(new TypedSymbol(parameter))))})
            {{
                {string.Join(", ", properties.Select(property => $"{property.Name} = {GenerateFieldValueReturn(new TypedSymbol(property))}"))}
            }};
        }}
        
        public static {_builder.Name} {_entity.Name} => new {_builder.Name}();
";

        }

        private string GenerateFieldValueReturn(TypedSymbol property)
            => IsMockable(property.Type)
                ? string.Format(_mockingConfiguration!.ReturnObjectFormat, property.Symbol.UnderScoreName())
                : property.Symbol.UnderScoreName();
    }
}