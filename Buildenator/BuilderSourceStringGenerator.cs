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
        private readonly FixtureConfiguration _fixtureConfiguration;

        public BuilderSourceStringGenerator(
            BuilderProperties builder,
            EntityToBuildProperties entity,
            FixtureConfiguration fixtureConfiguration)
        {
            _builder = builder;
            _entity = entity;
            _fixtureConfiguration = fixtureConfiguration;
        }

        public string CreateBuilderCode()
             => $@"
{GenerateNamespaces()}

namespace {_builder.ContainingNamespace}
{{
    public partial class {_builder.Name}
    {{
        private readonly {_fixtureConfiguration.Name} _fixture = new {_fixtureConfiguration.Name}();
{GenerateConstructor()}
{GeneratePropertiesCode()}
{GenerateBuildsCode()}
    }}
}}";

        private string GenerateNamespaces()
        {
            var list = new List<string>
            {
                "System",
                _fixtureConfiguration.Namespace,
                _entity.ContainingNamespace
            };
            var output = new StringBuilder();
            foreach (var @namespace in list.Concat(_fixtureConfiguration.AdditionalNamespaces).Distinct())
            {
                output.AppendLine($"using {@namespace};");
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
                output.AppendLine($@"            {parameter.UnderScoreName()} = _fixture.Create<{type}>();");
            }
            output.AppendLine($@"
        }}");
            return output.ToString();
        }

        private string GeneratePropertiesCode()
        {
            var properties = _entity.GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();

            foreach (var (property, type) in properties
                .Where(x => !_builder.Fields.TryGetValue(x.Property.UnderScoreName(), out var field) || field.Type.Name != x.Type.Name))
            {
                output.AppendLine($@"        private {type} {property.UnderScoreName()};");
            }

            foreach (var (property, type) in properties.Where(x => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x.Property), out var method)
                 || !(method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.Type.Name)))
            {
                output.AppendLine($@"

        public {_builder.Name} {CreateMethodName(property)}({type} value)
        {{
            {property.UnderScoreName()} = value;
            return this;
        }}");

            }

            return output.ToString();

            string CreateMethodName(ISymbol property) => $"{_builder.BuildingMethodsPrefix}{property.PascalCaseName()}";
        }

        private string GenerateBuildsCode()
        {
            var parameters = _entity.ConstructorParameters;
            var properties = _entity.SettableProperties
                .Where(x => !parameters.ContainsKey(x.Name));

            return $@"        public {_entity.Name} Build()
        {{
            return new {_entity.Name}({string.Join(", ", parameters.Values.Select(parameter => parameter.UnderScoreName()))})
            {{
                {string.Join(", ", properties.Select(property => $"{property.Name} = {property.UnderScoreName()}"))}
            }};
        }}
        
        public static {_builder.Name} {_entity.Name} => new {_builder.Name}();
";

        }
    }
}