using System;

namespace Buildenator.Abstraction
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class FixtureConfigurationAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fixtureType"></param>
        /// <param name="additionalUsings">List all the additional namespaces that are important for the fixture; separate them by comma ','. 
        /// An example: "Namespace1,Namespace2.Subspace"</param>
        public FixtureConfigurationAttribute(Type fixtureType, FixtureInterfacesStrategy strategy = FixtureInterfacesStrategy.OnlyGenericCollections, string? additionalUsings = null)
        {
            FixtureType = fixtureType;
            Strategy = strategy;
            AdditionalUsings = additionalUsings?.Split(',') ?? Array.Empty<string>();
        }

        public Type FixtureType { get; }
        public FixtureInterfacesStrategy Strategy { get; }
        public string[] AdditionalUsings { get; }
    }
}
