using System;

namespace Buildenator.Abstraction.AutoFixture
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class AutoFixtureWithMoqConfigurationAttribute : FixtureConfigurationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="additionalUsings">List all the additional namespaces that are important for the fixture; separate them by comma ','. 
        /// An example: "Namespace1,Namespace2.Subspace"</param>
        public AutoFixtureWithMoqConfigurationAttribute(
            string fixtureTypeName = "Fixture",
            string createSingleFormat = "Create<{0}>()",
            string? constructorParameters = null,
            string? additionalConfiguration = "{0} = {0}.Customize(new AutoMoqCustomization()))",
            FixtureInterfacesStrategy strategy = FixtureInterfacesStrategy.OnlyGenericCollections,
            string? additionalUsings = "AutoFixture,AutoFixture.AutoMoq")
            :base(fixtureTypeName, createSingleFormat, constructorParameters, additionalConfiguration, strategy, additionalUsings)
        {
        }
    }
}
