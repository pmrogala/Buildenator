using Buildenator.Abstraction;
using Buildenator.Configuration;
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
{GenerateGlobalNullable()}{GenerateBuilderDefinition()}
    {{
{(_fixtureConfiguration is null ? string.Empty : $"        private readonly {_fixtureConfiguration.Name} {FixtureLiteral} = new {_fixtureConfiguration.Name}({_fixtureConfiguration.ConstructorParameters});")}
{GenerateConstructor()}
{GeneratePropertiesCode()}
{GenerateBuildsCode()}
{GenerateBuildManyCode()}
{(_builder.StaticCreator ? GenerateStaticBuildsCode() : string.Empty)}
    }}
}}";

        private string GenerateGlobalNullable()
            => _builder.NullableStrategy switch
            {
                NullableStrategy.Enabled => "#nullable enable\n",
                NullableStrategy.Disabled => "#nullable disable\n",
                _ => string.Empty
            };

        private string GenerateBuilderDefinition()
            => @$"    public partial class {_entity.FullNameWithConstraints.Replace(_entity.Name, _builder.Name)}";

        private string GenerateNamespaces()
        {
            var list = new string[]
            {
                "System",
                "System.Linq",
                "Buildenator.Abstraction.Helpers",
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
            foreach (var typedSymbol in parameters.Where(a => a.IsMockable()))
            {
                output.AppendLine($@"            {typedSymbol.UnderScoreName} = {GenerateMockedFieldInitialization(typedSymbol)};");
            }

            if (_fixtureConfiguration is not null && _fixtureConfiguration.AdditionalConfiguration is not null)
            {
                output.AppendLine($@"            {string.Format(_fixtureConfiguration.AdditionalConfiguration, FixtureLiteral)};");
            }

            output.AppendLine($@"
        }}");

            return output.ToString();
        }

        private string GenerateMockedFieldInitialization(TypedSymbol typedSymbol)
            => string.Format(_mockingConfiguration!.FieldDeafultValueAssigmentFormat, typedSymbol.TypeFullName);

        private string GeneratePropertiesCode()
        {
            var properties = _entity.GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();

            foreach (var typedSymbol in properties.Where(IsNotYetDeclaredField))
            {
                output.AppendLine($@"        private {GenerateLazyFieldType(typedSymbol)} {typedSymbol.UnderScoreName};");
            }

            foreach (var typedSymbol in properties.Where(IsNotYetDeclaredMethod))
            {
                output.AppendLine($@"

        {GenerateMethodDefinition(typedSymbol)}
        {{
            {GenerateValueAssigment(typedSymbol)};
            return this;
        }}");

            }

            return output.ToString();

            bool IsNotYetDeclaredField(TypedSymbol x) => !_builder.Fields.TryGetValue(x.UnderScoreName, out var field);

            bool IsNotYetDeclaredMethod(TypedSymbol x) => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x), out var method)
                                 || !(method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.TypeName);
        }

        private string GenerateValueAssigment(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable()
                ? $"{SetupActionLiteral}({typedSymbol.UnderScoreName})"
                : $"{typedSymbol.UnderScoreName} = new Nullbox<{typedSymbol.TypeFullName}>({ValueLiteral})";

        private string CreateMethodName(TypedSymbol property) => $"{_builder.BuildingMethodsPrefix}{property.SymbolPascalName}";

        private string GenerateMethodDefinition(TypedSymbol typedSymbol)
            => $"public {_builder.FullName} {CreateMethodName(typedSymbol)}({GenerateMethodParameterDefinition(typedSymbol)})";

        private string GenerateMethodParameterDefinition(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable() ? $"Action<{CreateMockableFieldType(typedSymbol)}> {SetupActionLiteral}" : $"{typedSymbol.TypeFullName} {ValueLiteral}";

        private string GenerateLazyFieldType(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable() ? CreateMockableFieldType(typedSymbol) : $"Nullbox<{typedSymbol.TypeFullName}>?";

        private string GenerateFieldType(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable() ? CreateMockableFieldType(typedSymbol) : typedSymbol.TypeFullName;

        private string CreateMockableFieldType(TypedSymbol type) => string.Format(_mockingConfiguration!.TypeDeclarationFormat, type.TypeFullName);

        private string GenerateBuildsCode()
        {
            var (parameters, properties) = GetParametersAndProperties();

            string disableWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning disable CS8604\n"
                : string.Empty;
            string restoreWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning restore CS8604\n"
                : string.Empty;

            return $@"{disableWarning}        public {_entity.FullName} Build()
        {{
            {GenerateLazyBuildEntityString(parameters, properties)}
        }}
{restoreWarning}
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
                    return $"{fieldType} {s.UnderScoreName} = default({fieldType})";
                }).ComaJoin();
            string disableWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning disable CS8625\n"
                : string.Empty;
            string restoreWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning restore CS8625\n"
                : string.Empty;

            return $@"{disableWarning}        public static {_entity.FullName} BuildDefault({methodParameters})
        {{
            {GenerateBuildEntityString(parameters, properties)}
        }}
{restoreWarning}";

        }

        private (IEnumerable<TypedSymbol> Parameters, IEnumerable<TypedSymbol> Properties) GetParametersAndProperties()
        {
            var parameters = _entity.ConstructorParameters;
            var properties = _entity.SettableProperties.Where(x => !parameters.ContainsKey(x.SymbolName));
            return (parameters.Values, properties);
        }

        private string GenerateLazyBuildEntityString(IEnumerable<TypedSymbol> parameters, IEnumerable<TypedSymbol> properties)
        {
            string propertiesAssigment = properties.Select(property => $"{property.SymbolName} = {GenerateLazyFieldValueReturn(property)}").ComaJoin();
            return @$"return new {_entity.FullName}({parameters.Select(parameter => GenerateLazyFieldValueReturn(parameter)).ComaJoin()})
            {{
{(string.IsNullOrEmpty(propertiesAssigment) ? string.Empty : $"                {propertiesAssigment}")}
            }};";
        }

        private string GenerateBuildEntityString(IEnumerable<TypedSymbol> parameters, IEnumerable<TypedSymbol> properties)
        {
            string propertiesAssigment = properties.Select(property => $"{property.SymbolName} = {GenerateFieldValueReturn(property)}").ComaJoin();
            return @$"return new {_entity.FullName}({parameters.Select(parameter => GenerateFieldValueReturn(parameter)).ComaJoin()})
            {{
{(string.IsNullOrEmpty(propertiesAssigment) ? string.Empty : $"                {propertiesAssigment}")}
            }};";
        }

        private string GenerateLazyFieldValueReturn(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable()
                ? string.Format(_mockingConfiguration!.ReturnObjectFormat, typedSymbol.UnderScoreName)
                : @$"({typedSymbol.UnderScoreName}.HasValue ? {typedSymbol.UnderScoreName}.Value : new Nullbox<{typedSymbol.TypeFullName}>({(typedSymbol.IsFakeable()
                    ? $"{FixtureLiteral}.{string.Format(_fixtureConfiguration!.CreateSingleFormat, typedSymbol.TypeFullName)}"
                    : $"default({typedSymbol.TypeFullName})")})).Object";

        private string GenerateFieldValueReturn(TypedSymbol typedSymbol)
            => typedSymbol.IsMockable()
                ? string.Format(_mockingConfiguration!.ReturnObjectFormat, typedSymbol.UnderScoreName)
                : typedSymbol.UnderScoreName;
    }

}