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
            var parameters = GetConstructorParameters();

            var output = new StringBuilder();
            output.AppendLine($@"
        public {_builder.Name}()
        {{");
            foreach (var parameter in parameters)
            {
                output.AppendLine($@"            {parameter.UnderScoreName()} = _fixture.Create<{parameter.Type}>();");
            }
            output.AppendLine($@"
        }}");
            return output.ToString();
        }

        private string GeneratePropertiesCode()
        {
            var parameters = GetConstructorParameters();

            var output = new StringBuilder();

            foreach (var parameter in parameters)
            {
                output.AppendLine($@"

        private {parameter.Type} {parameter.UnderScoreName()};

        public {_builder.Name} With{parameter.PascalCaseName()}({parameter.Type} value)
        {{
            {parameter.UnderScoreName()} = value;
            return this;
        }}");

            }

            return output.ToString();
        }

        private string GenerateBuildsCode()
        {
            var parameters = GetConstructorParameters();

            var output = new StringBuilder();

            output.AppendLine($@"        public {_classToBuild.Name} Build()
        {{
            return new {_classToBuild.Name}(");

            output.Append(string.Join(", ", parameters.Select(parameter => parameter.UnderScoreName())));

            output.AppendLine($@");
        }}
        
        public static {_builder.Name} {_classToBuild.Name} => new {_builder.Name}();");

            return output.ToString();

        }

        private IEnumerable<IParameterSymbol> GetConstructorParameters()
        {
            var properties = _classToBuild.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters;
            var propertyNames = properties.Select(x => x.Name);

            var baseType = _classToBuild.BaseType;

            return properties;
        }

        private IEnumerable<IPropertySymbol> GetSetableProperties()
        {
            var properties = _classToBuild.GetMembers().OfType<IPropertySymbol>()
                .Where(x => x.SetMethod is not null)
                .Where(x => x.CanBeReferencedByName).ToList();
            var propertyNames = properties.Select(x => x.Name);

            var baseType = _classToBuild.BaseType;

            while (baseType != null)
            {

                properties.AddRange(baseType.GetMembers().OfType<IPropertySymbol>()
                                            .Where(x => x.CanBeReferencedByName)
                                            .Where(x => x.SetMethod is not null)
                                            .Where(x => !propertyNames.Contains(x.Name)));

                baseType = baseType.BaseType;
            }

            return properties;
        }
    }
}