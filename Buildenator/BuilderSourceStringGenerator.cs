using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
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
             => $@"{GenerateNamespaces()}

namespace {_builder.ContainingNamespace}
{{
{GenerateBuilderDefinition()}
    {{
{(_fixtureConfiguration is null ? string.Empty : $"        private readonly {_fixtureConfiguration.Name} {FixtureLiteral} = new {_fixtureConfiguration.Name}({_fixtureConfiguration.ConstructorParameters});")}
{GenerateConstructor()}
{GeneratePropertiesCode()}
{GenerateBuildsCode()}
{GenerateBuildManyCode()}
{(_builder.StaticCreator ? GenerateStaticBuildsCode() : string.Empty)}
    }}
}}";

        private string GenerateBuilderDefinition()
            => @$"    public partial class {_entity.FullNameWithConstraints.Replace(_entity.Name, _builder.Name)}";

        private string GenerateNamespaces()
        {
            var list = new string[]
            {
                "System",
                "System.Linq",
                _entity.ContainingNamespace
            }
                .Concat(_fixtureConfiguration?.AdditionalNamespaces ?? Enumerable.Empty<string>())
                .Concat(_mockingConfiguration?.AdditionalNamespaces ?? Enumerable.Empty<string>())
                .Concat(_entity?.AdditionalNamespaces ?? Enumerable.Empty<string>());

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
            foreach (var typedSymbol in parameters)
            {
                output.AppendLine($@"            {typedSymbol.UnderScoreName()} = {GenerateCreatingFieldInitialization(typedSymbol)};");
            }

            if (_fixtureConfiguration is not null && _fixtureConfiguration.AdditionalConfiguration is not null)
            {
                output.AppendLine($@"            {string.Format(_fixtureConfiguration.AdditionalConfiguration, FixtureLiteral)};");
            }

            output.AppendLine($@"
        }}");

            return output.ToString();
        }

        private string GenerateCreatingFieldInitialization(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable()
                ? string.Format(_mockingConfiguration!.FieldDeafultValueAssigmentFormat, typedSymbol.Type.ToDisplayString())
                : typedSymbol.IsFakeable()
                    ? $"{FixtureLiteral}.{string.Format(_fixtureConfiguration!.CreateSingleFormat, typedSymbol.Type.ToDisplayString())}"
                    : $"default({typedSymbol.Type.ToDisplayString()})";

        private string GeneratePropertiesCode()
        {
            var properties = _entity.GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();

            foreach (var typedSymbol in properties
                .Where(x => !_builder.Fields.TryGetValue(x.UnderScoreName(), out var field) || field.Type.Name != x.Type.Name))
            {
                output.AppendLine($@"        private {GenerateFieldType(typedSymbol)} {typedSymbol.UnderScoreName()};");
            }

            foreach (var typedSymbol in properties.Where(x => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x.Symbol), out var method)
                 || !(method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.Type.Name)))
            {
                output.AppendLine($@"

        {GenerateMethodDefinition(typedSymbol)}
        {{
            {GenerateValueAssigment(typedSymbol)};
            return this;
        }}");

            }

            return output.ToString();
        }

        private string GenerateValueAssigment(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable()
                ? $"{SetupActionLiteral}({typedSymbol.UnderScoreName()})"
                : $"{typedSymbol.UnderScoreName()} = {ValueLiteral}";

        private string CreateMethodName(ISymbol property) => $"{_builder.BuildingMethodsPrefix}{property.PascalCaseName()}";

        private string GenerateMethodDefinition(TypedSymbol typedSymbol)
            => $"public {_builder.FullName} {CreateMethodName(typedSymbol.Symbol)}({GenerateMethodParameterDefinition(typedSymbol)})";

        private string GenerateMethodParameterDefinition(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable() ? $"Action<{CreateMockableFieldType(typedSymbol.Type)}> {SetupActionLiteral}" : $"{typedSymbol.Type} {ValueLiteral}";

        private string GenerateFieldType(TypedSymbol typedSymbol) => typedSymbol.IsMockable() ? CreateMockableFieldType(typedSymbol.Type) : typedSymbol.Type.ToDisplayString();

        private string CreateMockableFieldType(ITypeSymbol type) => string.Format(_mockingConfiguration!.TypeDeclarationFormat, type.ToDisplayString());

        private string GenerateBuildsCode()
        {
            var (parameters, properties) = GetParametersAndProperties();

            return $@"        public {_entity.FullName} Build()
        {{
            {GenerateBuildEntityString(parameters, properties)}
        }}

        public static {_builder.FullName} {_entity.Name} => new {_builder.FullName}();
";

        }
        private string GenerateBuildManyCode()
        {
            return $@"        public System.Collections.Generic.IEnumerable<{_entity.FullName}> BuildMany(int count = 3)
        {{
            return Enumerable.Range(0, count).Select(_ => Build());
        }}
";

        }
        private string GenerateStaticBuildsCode()
        {
            var (parameters, properties) = GetParametersAndProperties();

            string methodParameters = parameters
                .Concat(properties)
                .Select(s =>
                {
                    var fieldType = GenerateFieldType(s);
                    return $"{fieldType} {s.Symbol.UnderScoreName()} = default({fieldType})";
                }).ComaJoin();
            return $@"        public static {_entity.FullName} BuildDefault({methodParameters})
        {{
            {GenerateBuildEntityString(parameters, properties)}
        }}
";

        }

        private (IEnumerable<TypedSymbol> Parameters, IEnumerable<TypedSymbol> Properties) GetParametersAndProperties()
        {
            var parameters = _entity.ConstructorParameters;
            var properties = _entity.SettableProperties.Where(x => !parameters.ContainsKey(x.Symbol.Name));
            return (parameters.Values, properties);
        }

        private string GenerateBuildEntityString(IEnumerable<TypedSymbol> parameters, IEnumerable<TypedSymbol> properties)
        {
            string propertiesAssigment = properties.Select(property => $"{property.Symbol.Name} = {GenerateFieldValueReturn(property)}").ComaJoin();
            return @$"return new {_entity.FullName}({parameters.Select(parameter => GenerateFieldValueReturn(parameter)).ComaJoin()})
            {{
{(string.IsNullOrEmpty(propertiesAssigment) ? string.Empty : $"                {propertiesAssigment}")}
            }};";
        }

        private string GenerateFieldValueReturn(TypedSymbol property)
            => property.IsMockable()
                ? string.Format(_mockingConfiguration!.ReturnObjectFormat, property.UnderScoreName())
                : property.UnderScoreName();
    }

}