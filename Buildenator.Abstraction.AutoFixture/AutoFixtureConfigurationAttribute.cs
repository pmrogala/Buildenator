using System;

namespace Buildenator.Abstraction.AutoFixture
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false)]
    public class AutoFixtureConfigurationAttribute : FixtureConfigurationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public AutoFixtureConfigurationAttribute(
            string fixtureTypeName = "Fixture",
            string createSingleFormat = "{2}.Create<{0}>()",
            string? constructorParameters = null,
            string? additionalConfiguration = null,
            FixtureInterfacesStrategy strategy = FixtureInterfacesStrategy.OnlyGenericCollections,
            string? additionalUsings = "AutoFixture")
            :base(fixtureTypeName, createSingleFormat, constructorParameters, additionalConfiguration, strategy, additionalUsings)
        {
        }
    }
}
