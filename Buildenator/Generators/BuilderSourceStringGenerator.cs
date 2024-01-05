using Buildenator.Abstraction;
using Buildenator.CodeAnalysis;
using Buildenator.Configuration.Contract;
using Buildenator.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buildenator.Configuration;
using static Buildenator.Generators.NamespacesGenerator;
using static Buildenator.Generators.ConstructorsGenerator;

namespace Buildenator.Generators
{
	internal sealed class BuilderSourceStringGenerator
    {
        private readonly IBuilderProperties _builder;
        private readonly IEntityToBuild _entity;
        private readonly IFixtureProperties? _fixtureConfiguration;
        private readonly IMockingProperties? _mockingConfiguration;
        private readonly PropertiesStringGenerator _propertiesStringGenerator;

        public BuilderSourceStringGenerator(
            IBuilderProperties builder,
            IEntityToBuild entity,
            IFixtureProperties? fixtureConfiguration,
            IMockingProperties? mockingConfiguration)
        {
            _builder = builder;
            _entity = entity;
            _fixtureConfiguration = fixtureConfiguration;
            _mockingConfiguration = mockingConfiguration;
            _propertiesStringGenerator = new PropertiesStringGenerator(_builder, _entity);
        }

        public string CreateBuilderCode()
             => $@"{AutoGenerationComment}
{GenerateNamespaces(_fixtureConfiguration, _mockingConfiguration, _entity)}

namespace {_builder.ContainingNamespace}
{{
{GenerateGlobalNullable()}{GenerateBuilderDefinition()}
    {{
{(_fixtureConfiguration is null ? string.Empty : $"        private readonly {_fixtureConfiguration.Name} {DefaultConstants.FixtureLiteral} = new {_fixtureConfiguration.Name}({_fixtureConfiguration.ConstructorParameters});")}
{(_builder.IsDefaultConstructorOverriden ? string.Empty : GenerateConstructor(_builder.Name, _entity, _fixtureConfiguration))}
{_propertiesStringGenerator.GeneratePropertiesCode()}
{GenerateBuildsCode()}
{GenerateBuildManyCode()}
{(_builder.StaticCreator ? GenerateStaticBuildsCode() : string.Empty)}
{(_builder.ImplicitCast ? GenerateImplicitCastCode() : string.Empty)}
{GeneratePostBuildMethod()}
    }}
}}";

        private object GeneratePostBuildMethod()
            => _builder.IsPostBuildMethodOverriden
            ? string.Empty
            : @$"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public void PostBuild({_entity.FullName} buildResult) {{ }}";

        private string GenerateGlobalNullable()
            => _builder.NullableStrategy switch
            {
                NullableStrategy.Enabled => "#nullable enable\n",
                NullableStrategy.Disabled => "#nullable disable\n",
                _ => string.Empty
            };

        private string GenerateBuilderDefinition()
	        => @$"    public partial class {_entity.FullNameWithConstraints.Replace(_entity.Name, _builder.Name)}";

        private string GenerateBuildsCode()
        {
            var (parameters, properties) = GetParametersAndProperties();

            var disableWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning disable CS8604\n"
                : string.Empty;
            var restoreWarning = _builder.NullableStrategy == NullableStrategy.Enabled
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

            var methodParameters = parameters
                .Concat(properties)
                .Select(s =>
                {
                    var fieldType = s.GenerateFieldType();
                    return $"{fieldType} {s.UnderScoreName} = default({fieldType})";
                }).ComaJoin();
            var disableWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning disable CS8625\n"
                : string.Empty;
            var restoreWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning restore CS8625\n"
                : string.Empty;

            return $@"{disableWarning}        public static {_entity.FullName} BuildDefault({methodParameters})
        {{
            {GenerateBuildEntityString(parameters, properties)}
        }}
{restoreWarning}";

        }
        
        private string GenerateImplicitCastCode()
        {
            return $@"        public static implicit operator {_entity.FullName}({_builder.FullName} builder) => builder.Build();";
        }

        private (IReadOnlyList<ITypedSymbol> Parameters, IReadOnlyList<ITypedSymbol> Properties) GetParametersAndProperties()
        {
            var parameters = _entity.ConstructorParameters;
            var properties = _entity.SettableProperties.Where(x => !parameters.ContainsKey(x.SymbolName));
            return (parameters.Values.ToList(), properties.ToList());
        }

        private string GenerateLazyBuildEntityString(IEnumerable<ITypedSymbol> parameters, IEnumerable<ITypedSymbol> properties)
        {
            var propertiesAssignment = properties.Select(property => $"{property.SymbolName} = {property.GenerateLazyFieldValueReturn()}").ComaJoin();
            return @$"var result = new {_entity.FullName}({parameters.Select(symbol => symbol.GenerateLazyFieldValueReturn()).ComaJoin()})
            {{
{(string.IsNullOrEmpty(propertiesAssignment) ? string.Empty : $"                {propertiesAssignment}")}
            }};
            {(_builder.ShouldGenerateMethodsForUnreachableProperties ? GenerateUnreachableProperties(): "")}
            PostBuild(result);
            return result;";

            string GenerateUnreachableProperties()
            {
                var output = new StringBuilder();
                output.AppendLine($"var t = typeof({_entity.FullName});");
                foreach (var a in _entity.GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch())
                {
                    output.Append($"            t.GetProperty(\"{a.SymbolName}\")")
                        .Append(_builder.NullableStrategy == NullableStrategy.Enabled ? "!" : "")
                        .AppendLine($".SetValue(result, {a.GenerateLazyFieldValueReturn()}, System.Reflection.BindingFlags.NonPublic, null, null, null);");
                }
                return output.ToString();
            }
        }

        private string GenerateBuildEntityString(IEnumerable<ITypedSymbol> parameters, IEnumerable<ITypedSymbol> properties)
        {
            var propertiesAssignment = properties.Select(property => $"{property.SymbolName} = {GenerateFieldValueReturn(property)}").ComaJoin();
            return @$"return new {_entity.FullName}({parameters.Select(GenerateFieldValueReturn).ComaJoin()})
            {{
{(string.IsNullOrEmpty(propertiesAssignment) ? string.Empty : $"                {propertiesAssignment}")}
            }};";
        }

        private string GenerateFieldValueReturn(ITypedSymbol typedSymbol)
            => typedSymbol.IsMockable()
                ? string.Format(_mockingConfiguration!.ReturnObjectFormat, typedSymbol.UnderScoreName)
                : typedSymbol.UnderScoreName;

        private const string AutoGenerationComment = @"
// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a source generator named Buildenator (https://github.com/progala2/Buildenator)
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------";
    }
}