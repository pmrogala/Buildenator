using Buildenator.CodeAnalysis;
using Buildenator.Configuration.Contract;
using System.Linq;
using System.Text;

namespace Buildenator.Generators
{
    internal static class ConstructorsGenerator
    {
        private const string FixtureLiteral = "_fixture";

        internal static string GenerateConstructor(
            string builderName,
            IEntityToBuildProperties _entity,
            IMockingProperties? _mockingConfiguration,
            IFixtureProperties? _fixtureConfiguration)
        {
            var parameters = _entity.GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();
            output.AppendLine($@"
        public {builderName}()
        {{");
            foreach (var typedSymbol in parameters.Where(a => a.IsMockable()))
            {
                output.AppendLine($@"            {typedSymbol.UnderScoreName} = {GenerateMockedFieldInitialization(typedSymbol)};");
            }

            if (_fixtureConfiguration is not null && _fixtureConfiguration.AdditionalConfiguration is not null)
            {
                output.AppendLine($@"            {string.Format(_fixtureConfiguration.AdditionalConfiguration, FixtureLiteral, _fixtureConfiguration.Name)};");
            }

            output.AppendLine($@"
        }}");

            return output.ToString();

            string GenerateMockedFieldInitialization(ITypedSymbol typedSymbol)
                => string.Format(_mockingConfiguration!.FieldDeafultValueAssigmentFormat, typedSymbol.TypeFullName);
        }
    }
}