using System;

namespace Buildenator.Abstraction.AutoFixture
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false)]
    public class AutoFixtureWithMoqConfigurationAttribute : FixtureConfigurationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public AutoFixtureWithMoqConfigurationAttribute(
            string fixtureTypeName = "Fixture",
            string createSingleFormat = "{2}.Create<{0}>()",
            string? constructorParameters = null,
            string? additionalConfiguration = "{0} = ({1}){0}.Customize(new AutoMoqCustomization())",
            FixtureInterfacesStrategy strategy = FixtureInterfacesStrategy.All,
            string? additionalUsings = "AutoFixture,AutoFixture.AutoMoq")
            :base(fixtureTypeName, createSingleFormat, constructorParameters, additionalConfiguration, strategy, additionalUsings)
        {
        }
    }
}
