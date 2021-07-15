using System;

namespace Buildenator.Abstraction.AutoFixture
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class AutoFixtureConfigurationAttribute : FixtureConfigurationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="additionalUsings">List all the additional namespaces that are important for the fixture; separate them by comma ','. 
        /// An example: "Namespace1,Namespace2.Subspace"</param>
        public AutoFixtureConfigurationAttribute(
            string fixtureTypeName = "Fixture",
            string createSingleFormat = "Create<{0}>()",
            string? constructorParameters = null,
            string? additionalConfiguration = null,
            FixtureInterfacesStrategy strategy = FixtureInterfacesStrategy.OnlyGenericCollections,
            string? additionalUsings = "AutoFixture")
            :base(fixtureTypeName, createSingleFormat, constructorParameters, additionalConfiguration, strategy, additionalUsings)
        {
        }
    }
}
