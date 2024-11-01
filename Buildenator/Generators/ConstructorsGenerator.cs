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
            var hasAnyBody = false;
            var parameters = entity.AllUniqueSettablePropertiesAndParameters;

            var output = new StringBuilder();
        output = output.AppendLine($@"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public {builderName}()
        {{");
            foreach (var typedSymbol in parameters.Where(a => a.NeedsFieldInit()))
            {
                output = output.AppendLine($@"            {typedSymbol.GenerateFieldInitialization()}");
                hasAnyBody = true;
            }

            if (fixtureConfiguration is not null && fixtureConfiguration.NeedsAdditionalConfiguration())
            {
                output = output.AppendLine($@"            {fixtureConfiguration.GenerateAdditionalConfiguration()};");
                hasAnyBody = true;
            }

            output = output.AppendLine($@"
        }}");

            return hasAnyBody ? output.ToString() : string.Empty;
        }
}