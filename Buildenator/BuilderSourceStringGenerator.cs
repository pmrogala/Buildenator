using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buildenator
{
    public class BuilderSourceStringGenerator
    {
        private readonly INamedTypeSymbol _builder;
        private readonly INamedTypeSymbol _classToBuild;

        public BuilderSourceStringGenerator(INamedTypeSymbol builder, INamedTypeSymbol classToBuild)
        {
            _builder = builder;
            _classToBuild = classToBuild;
        }

        public string CreateBuilderCode()
             => $@"
using System;
using AutoFixture;
using {_classToBuild.ContainingNamespace};

namespace {_builder.ContainingNamespace}
{{
    public partial class {_builder.Name}
    {{
        private readonly Fixture _fixture = new Fixture();
{GenerateConstructor()}
{GeneratePropertiesCode()}
{GenerateBuildsCode()}
    }}
}}";

        private string GenerateConstructor()
        {
            var parameters = GetAllUniqueSettablePropertiesAndParameters();

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
            var properties = GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();

            foreach (var (property, type) in properties)
            {
                output.AppendLine($@"

        private {type} {property.UnderScoreName()};

        public {_builder.Name} With{property.PascalCaseName()}({type} value)
        {{
            {property.UnderScoreName()} = value;
            return this;
        }}");

            }

            return output.ToString();
        }

        private IEnumerable<(ISymbol, ITypeSymbol Type)> GetAllUniqueSettablePropertiesAndParameters()
        {
            var parameters = GetConstructorParameters();
            return GetSetableProperties()
                .Where(x => !parameters.ContainsKey(x.Name))
                .Select(x => ((ISymbol)x, x.Type))
                .Concat(parameters.Values.Select(x => ((ISymbol)x, x.Type)));
        }

        private string GenerateBuildsCode()
        {
            var parameters = GetConstructorParameters();
            var properties = GetSetableProperties()
                .Where(x => !parameters.ContainsKey(x.Name));

            return $@"        public {_classToBuild.Name} Build()
        {{
            return new {_classToBuild.Name}({string.Join(", ", parameters.Values.Select(parameter => parameter.UnderScoreName()))})
            {{
                {string.Join(", ", properties.Select(property => $"{property.Name} = {property.UnderScoreName()}"))}
            }};
        }}
        
        public static {_builder.Name} {_classToBuild.Name} => new {_builder.Name}();
";

        }

        private IReadOnlyDictionary<string, IParameterSymbol> GetConstructorParameters()
        {
            var properties = _classToBuild.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters;

            return properties.ToDictionary(x => x.PascalCaseName());
        }

        private IEnumerable<IPropertySymbol> GetSetableProperties()
        {
            var properties = _classToBuild.GetMembers().OfType<IPropertySymbol>()
                .Where(x => x.SetMethod is not null)
                .Where(x => x.SetMethod!.DeclaredAccessibility == Accessibility.Public)
                .Where(x => x.CanBeReferencedByName).ToList();

            var propertyNames = new HashSet<string>(properties.Select(x => x.Name));

            var baseType = _classToBuild.BaseType;

            while (baseType != null)
            {
                var newProperties = baseType.GetMembers().OfType<IPropertySymbol>()
                                            .Where(x => x.CanBeReferencedByName)
                                            .Where(x => x.SetMethod is not null)
                                            .Where(x => x.SetMethod!.DeclaredAccessibility == Accessibility.Public)
                                            .Where(x => !propertyNames.Contains(x.Name)).ToList();
                properties.AddRange(newProperties);
                propertyNames.UnionWith(newProperties.Select(x => x.Name));

                baseType = baseType.BaseType;
            }

            return properties;
        }
    }
}