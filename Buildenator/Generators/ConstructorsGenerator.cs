using Buildenator.Configuration;
using Buildenator.Configuration.Contract;
using System.Linq;
using System.Text;

namespace Buildenator.Generators;

internal static class ConstructorsGenerator
{
    internal static string GenerateConstructor(
        string builderName,
        IEntityToBuild entity,
        IFixtureProperties? fixtureConfiguration)
    {
            var parameters = entity.AllUniqueSettablePropertiesAndParameters;

            var output = new StringBuilder();
        output = output.AppendLine($@"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public {builderName}()
        {{");
            foreach (var typedSymbol in parameters.Where(a => a.NeedsFieldInit()))
            {
                output = output.AppendLine($@"            {typedSymbol.GenerateFieldInitialization()}");
            }

            if (fixtureConfiguration is not null && fixtureConfiguration.NeedsAdditionalConfiguration())
            {
                output = output.AppendLine($@"            {fixtureConfiguration.GenerateAdditionalConfiguration()};");
            }

            output = output.AppendLine($@"            {DefaultConstants.PreBuildMethodName}();");

            output = output.AppendLine($@"
        }}");

            return output.ToString();
        }
}