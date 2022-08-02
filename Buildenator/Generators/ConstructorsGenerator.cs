using Buildenator.Configuration.Contract;
using System.Linq;
using System.Text;

namespace Buildenator.Generators
{
    internal static class ConstructorsGenerator
    {
        internal static string GenerateConstructor(
            string builderName,
            IEntityToBuild _entity,
            IFixtureProperties? _fixtureConfiguration)
        {
            var hasAnyBody = false;
            var parameters = _entity.GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();
            output.AppendLine($@"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public {builderName}()
        {{");
            foreach (var typedSymbol in parameters.Where(a => a.NeedsFieldInit()))
            {
                output.AppendLine($@"            {typedSymbol.GenerateFieldInitialization()}");
                hasAnyBody = true;
            }

            if (_fixtureConfiguration is not null && _fixtureConfiguration.NeedsAdditionalConfiguration())
            {
                output.AppendLine($@"            {_fixtureConfiguration.GenerateAdditionalConfiguration()};");
                hasAnyBody = true;
            }

            output.AppendLine($@"
        }}");

            return hasAnyBody ? output.ToString() : string.Empty;
        }
    }
}